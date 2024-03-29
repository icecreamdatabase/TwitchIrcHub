using System.Collections.Specialized;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using TwitchIrcHub.ExternalApis.Discord;
using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.DataTypes;
using TwitchIrcHub.IrcBot.Irc.IrcPoolManager;
using Timer = System.Timers.Timer;

namespace TwitchIrcHub.IrcBot.Irc.IrcClient;

public class IrcClient : IIrcClient
{
    private readonly ILogger<IrcClient> _logger;

    private const string Server = "irc.chat.twitch.tv";
    private const int Port = 6667;

    private IIrcPoolManager _ircPoolManager = null!;
    private string _connectionId = string.Empty;

    /* Ping */
    private bool _awaitingPing;
    private readonly Timer _pingInterval = new(20000);

    /* Reconnect */
    private static readonly Random ReconnectRandom = new();
    private const int ReconnectMaxJitter = 500;
    private const int ReconnectMultiplier = 2000;
    private const int ReconnectMaxMultiplier = 8;
    private int _reconnectionAttempts;
    private bool _fullyConnected;

    /* Irc */
    private TcpClient? _tcpClient;
    private StreamWriter? _streamWriter;

    /* Threading and shutdown */
    private Thread _thread = null!;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _autoRestart = true;

    /* Channels and joining */
    public BulkObservableCollection<string> Channels { get; } = new();
    private readonly List<string> _actualChannels = new();
    private DateTime _lastChannelChangeFlushed = DateTime.UnixEpoch;
    private bool _currentlyUpdatingChannels;
    private bool _isSendOnlyConnection;

    public IrcClient(ILogger<IrcClient> logger)
    {
        _logger = logger;
        _pingInterval.Elapsed += PingIntervalOnElapsed;
        Channels.CollectionChanged += UpdateJoinedChannels;
    }

    public void Init(IIrcPoolManager ircPoolManager, bool isSendOnlyConnection, string connectionId)
    {
        _isSendOnlyConnection = isSendOnlyConnection;
        _ircPoolManager = ircPoolManager;
        _connectionId = connectionId;
        _thread = new Thread(ThreadRun);
        _thread.Start();
        _logger.LogInformation("{ConnId}: IrcClient Started", _connectionId);
    }

    private async void ThreadRun()
    {
        CancellationToken cancellationToken = _cancellationTokenSource.Token;

        while (!cancellationToken.IsCancellationRequested && _autoRestart)
        {
            await Task.Delay(500, cancellationToken);
            await Connect(cancellationToken);
        }

        _tcpClient?.Dispose();
    }

    private async void PingIntervalOnElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_awaitingPing)
        {
            _logger.LogInformation("{ConnId}: No PONG received for {Interval} s. Reconnecting...",
                _connectionId,
                _pingInterval.Interval / 1000);
            _pingInterval.Stop();
            await Disconnect();
            _awaitingPing = false;
        }
        else
        {
            if (_streamWriter != null && _fullyConnected)
            {
                try
                {
                    await _streamWriter.WriteLineAsync("PING");
                    await _streamWriter.FlushAsync();
                    _awaitingPing = true;
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        "{ConnId}: Not able to send a PONG because of {Exception}. Reconnecting...",
                        _connectionId,
                        exception.GetType().ToString()
                    );
                    _pingInterval.Stop();
                    await Disconnect();
                    _awaitingPing = false;
                }

                _awaitingPing = true;
            }
            else
            {
                _logger.LogWarning(
                    "{ConnId}: Not able to send a PONG. (_streamWriter != null == {StreamWriter}, _fullyConnected == {Fullyconnected}) Reconnecting...",
                    _connectionId,
                    _streamWriter != null, _fullyConnected
                );
                _pingInterval.Stop();
                await Disconnect();
                _awaitingPing = false;
            }
        }
    }

    private async Task Connect(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{ConnId}: Connecting to {Server}:{Port}", _connectionId, Server, Port);
        try
        {
            _tcpClient?.Close();
            _actualChannels.Clear();

            await _ircPoolManager.IrcBuckets.WaitForAuthenticateTicket(stoppingToken);

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(Server, Port, stoppingToken);

            await using NetworkStream stream = _tcpClient.GetStream();
            _streamWriter?.Close();
            _streamWriter = new StreamWriter(stream);
            using StreamReader reader = new StreamReader(stream);

            await _streamWriter.WriteLineAsync("CAP REQ :twitch.tv/tags twitch.tv/commands");
            // twitch.tv/membership")
            await _streamWriter.WriteLineAsync($"PASS oauth:{_ircPoolManager.BotOauth}");
            await _streamWriter.WriteLineAsync($"NICK {_ircPoolManager.BotUsername}");
            await _streamWriter.WriteLineAsync(
                $"USER {_ircPoolManager.BotUsername} 8 * :{_ircPoolManager.BotUsername}");
            await _streamWriter.FlushAsync();

            _pingInterval.Stop();
            _pingInterval.Start();
            _reconnectionAttempts = 0;

            while (_tcpClient.Connected && !stoppingToken.IsCancellationRequested)
            {
                string line;

                // This will make sure we can stop waiting when a cancellation token has been sent
                // https://devblogs.microsoft.com/pfxteam/how-do-i-cancel-non-cancelable-async-operations/
                Task<string?> readTask = reader.ReadLineAsync();
                try
                {
                    line = await readTask.WithCancellation(stoppingToken) ?? string.Empty;
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        "{ConnId}: Read was cancelled by {Exception}",
                        _connectionId, exception.GetType().ToString()
                    );
                    continue;
                }

                // No data = connection dead
                if (string.IsNullOrEmpty(line)) continue;
                IrcMessage? ircMessage = IrcParser.Parse(line);
                if (ircMessage == null) continue;

                await HandleIrcCommand(ircMessage, stoppingToken);
            }

            _streamWriter?.Close();
        }
        catch (SocketException e)
        {
            // ignored
            _logger.LogError("{ConnId}: {E}", _connectionId, e.ToString());
        }
        catch (IOException e)
        {
            // ignored
            _logger.LogError("{ConnId}: {E}", _connectionId, e.ToString());
        }
        catch (Exception e)
        {
            _logger.LogError("{ConnId}: {E}", _connectionId, e.ToString());
        }

        _streamWriter?.Close();
        _tcpClient?.Close();

        int reconnectionDelay = 150;
        if (_reconnectionAttempts > 0)
        {
            int randomJitter = ReconnectRandom.Next(ReconnectMaxJitter + 1);
            reconnectionDelay = ReconnectMultiplier * _reconnectionAttempts - ReconnectMaxJitter + randomJitter;
        }

        await Task.Delay(reconnectionDelay, CancellationToken.None);

        _reconnectionAttempts = Math.Min(_reconnectionAttempts, ReconnectMaxMultiplier);
    }

    public Task CheckAlive()
    {
        double totalSeconds = (DateTime.UtcNow - _lastReceivedLine).TotalSeconds;
        if (totalSeconds > 60)
        {
            _logger.LogWarning("{ConnId}: No lines received in the last {TotalSec} seconds", _connectionId,
                totalSeconds);
            DiscordLogger.Log(LogLevel.Warning, $"No lines received in the last {totalSeconds} seconds.");
        }

        return Task.CompletedTask;
    }

    private DateTime _lastReceivedLine = DateTime.UtcNow;

    private async Task HandleIrcCommand(IrcMessage ircMessage, CancellationToken stoppingToken)
    {
        _lastReceivedLine = DateTime.UtcNow;

        switch (ircMessage.IrcCommand)
        {
            /* --------------------------------------------------------------------------- */
            /* ------------------------------ Setup commands ----------------------------- */
            /* --------------------------------------------------------------------------- */
            case IrcCommands.RplWelcome when _isSendOnlyConnection:
                _fullyConnected = true;
                return;
            case IrcCommands.RplWelcome when !_isSendOnlyConnection:
                _ = Task.Delay(4000, stoppingToken).ContinueWith(_ =>
                {
                    _fullyConnected = true;
                    return UpdateJoinedChannels();
                }, stoppingToken);
                return;
            /* --------------------------------------------------------------------------- */
            /* ----------------------------- Ignored commands ---------------------------- */
            /* --------------------------------------------------------------------------- */
            case IrcCommands.RplYourHost:
            case IrcCommands.RplCreated:
            case IrcCommands.RplMyInfo:
            case IrcCommands.RplNamReply:
            case IrcCommands.RplEndOfNames:
            case IrcCommands.RplMotd:
            case IrcCommands.RplMotdStart:
            case IrcCommands.RplEndOfMotd:
            case IrcCommands.Cap:
                // Ignore
                return;
            /* --------------------------------------------------------------------------- */
            /* ---------------------- IrcClient management commands ---------------------- */
            /* --------------------------------------------------------------------------- */
            case IrcCommands.Ping when _streamWriter != null:
                await _streamWriter.WriteLineAsync("PONG");
                await _streamWriter.FlushAsync();
                return;
            case IrcCommands.Ping when _streamWriter == null:
                _logger.LogWarning("{ConnId}: Received Ping with no valid StreamWriter", _connectionId);
                return;
            case IrcCommands.Pong:
                _awaitingPing = false;
                return;
            case IrcCommands.Reconnect:
                // automatic restart by checking _tcpClient.Connected in the while loop
                _tcpClient?.Close();
                return;
            case IrcCommands.Join:
                string channelName = ircMessage.IrcParameters[0][1..];
                if (!_actualChannels.Contains(channelName))
                    _actualChannels.Add(channelName);
                return;
            case IrcCommands.Part:
                _actualChannels.Remove(ircMessage.IrcParameters[0][1..]);
                return;
            case IrcCommands.Notice
                when ircMessage.IrcParameters[1].Contains("Login authentication failed"):

                _logger.LogWarning("{ConnId}: Auth failed", _connectionId);
                //TODO: do we need to do something else here?
                await _ircPoolManager.ForceCheckAuth();
                return;
            /* --------------------------------------------------------------------------- */
            /* ---------------------- SignalR subscribable commands ---------------------- */
            /* --------------------------------------------------------------------------- */
            case IrcCommands.Notice:
            case IrcCommands.GlobalUserState:
            case IrcCommands.ClearChat:
            case IrcCommands.ClearMsg:
            case IrcCommands.HostTarget:
            case IrcCommands.UserState:
            case IrcCommands.RoomState:
            case IrcCommands.UserNotice:
            case IrcCommands.Whisper:
            case IrcCommands.PrivMsg:
                await _ircPoolManager.NewIrcMessage(ircMessage);
                return;
            /* --------------------------------------------------------------------------- */
            /* ----------------------------- Default fallback ---------------------------- */
            /* --------------------------------------------------------------------------- */
            default:
                //_logger.LogInformation(
                //    "{ConnId}: {Command}—{RawSource}",
                //    _connectionId, ircMessage.IrcCommand, ircMessage.RawSource
                //);
                return;
        }
    }

    private async Task Disconnect()
    {
        if (_streamWriter != null && _fullyConnected)
        {
            await _streamWriter.WriteLineAsync("QUIT");
            await _streamWriter.FlushAsync();
        }

        _fullyConnected = false;
        _pingInterval.Stop();
        _streamWriter?.Close();
        _tcpClient?.Close();
    }

    public async Task Shutdown()
    {
        _autoRestart = false;
        await Disconnect();
        _cancellationTokenSource.Cancel();
    }

    public async Task SendLine(string line)
    {
        if (_streamWriter == null)
            _logger.LogWarning("{ConnId}: StreamWriter is null for line: \n{Line}", _connectionId, line);
        else
            try
            {
                await _streamWriter.WriteLineAsync(line);
                await _streamWriter.FlushAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("{ConnId}: Failed sending line: \n{Line}\n{E}", _connectionId, line, e.ToString());
            }
    }

    private async void UpdateJoinedChannels(object? sender, NotifyCollectionChangedEventArgs args)
    {
        await UpdateJoinedChannels();
    }

    private async Task UpdateJoinedChannels()
    {
        // If we are only here for sending ... stop trying to join or part channels!
        if (_isSendOnlyConnection)
        {
            _logger.LogWarning("{ConnId}: Send only connection is trying to update channels!", _connectionId);
            return;
        }

        if (_tcpClient is { Connected: false } || !_fullyConnected || _currentlyUpdatingChannels) return;

        _currentlyUpdatingChannels = true;

        // Do {} while () as we always want the body to run at least once!
        do
        {
            await WaitForLastChannelFlushed();
        } while (await PartExcessive() || await JoinMissing());

        if (_actualChannels.Except(Channels).Any())
            _logger.LogWarning("{ConnId}: Still has channels to part: {Count}", _connectionId,
                _actualChannels.Except(Channels).Count());

        if (Channels.Except(_actualChannels).Any())
            _logger.LogWarning("{ConnId}: Still has channels to join: {Count}", _connectionId,
                Channels.Except(_actualChannels).Count());

        if (Channels.Count == 0)
        {
            _ircPoolManager.RemoveReceiveClient(this);
            await Shutdown();
        }

        _currentlyUpdatingChannels = false;
    }

    private async Task WaitForLastChannelFlushed()
    {
        while (DateTime.Now.Subtract(_lastChannelChangeFlushed).TotalSeconds < 10)
        {
            await Task.Delay(250);
        }
    }

    private async Task<bool> PartExcessive()
    {
        List<string> channelsToChange = _actualChannels.Except(Channels).ToList();
        bool hasChanged = false;

        while (channelsToChange.Any())
        {
            StringBuilder ircCommand = new StringBuilder("Part ");
            while (channelsToChange.Any() && ircCommand.Length < 450)
            {
                ircCommand.Append('#');
                ircCommand.Append(channelsToChange[0]);
                ircCommand.Append(',');
                channelsToChange.RemoveAt(0);
            }

            // We didn't actually join any channels
            if (ircCommand.Length <= 5) continue;

            string msg = ircCommand.ToString()[..(ircCommand.Length - 1)];
            _logger.LogInformation( "{ConnId} sending: {Msg}", _connectionId, msg );
            if (_streamWriter != null)
            {
                await _streamWriter.WriteLineAsync(msg);
                await _streamWriter.FlushAsync();
            }

            _lastChannelChangeFlushed = DateTime.Now;
            hasChanged = true;
        }

        return hasChanged;
    }

    private async Task<bool> JoinMissing()
    {
        List<string> channelsToChange = Channels.Except(_actualChannels).ToList();

        bool hasChanged = false;
        while (channelsToChange.Any() && _ircPoolManager.IrcBuckets.JoinBucket.TakeTicket())
        {
            StringBuilder ircCommand = new StringBuilder("Join ");
            while (channelsToChange.Any() && ircCommand.Length < 450)
            {
                ircCommand.Append('#');
                ircCommand.Append(channelsToChange[0]);
                ircCommand.Append(',');
                channelsToChange.RemoveAt(0);
            }

            // We didn't actually join any channels
            if (ircCommand.Length <= 5) continue;

            string msg = ircCommand.ToString()[..(ircCommand.Length - 1)];
            _logger.LogInformation("{ConnId} sending: {Msg}", _connectionId, msg);
            if (_streamWriter != null)
            {
                await _streamWriter.WriteLineAsync(msg);
                await _streamWriter.FlushAsync();
            }

            _lastChannelChangeFlushed = DateTime.Now;
            hasChanged = true;
        }

        while (!_ircPoolManager.IrcBuckets.JoinBucket.TicketAvailable && channelsToChange.Any())
            await Task.Delay(25);
        await Task.Delay(100);

        return hasChanged;
    }
}

public static class Cancellation
{
    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        TaskCompletionSource<bool> tcs = new();
        await using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            if (task != await Task.WhenAny(task, tcs.Task))
                throw new OperationCanceledException(cancellationToken);
        return await task;
    }
}
