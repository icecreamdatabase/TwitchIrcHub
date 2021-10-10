using System.Collections.Specialized;
using System.Net.Sockets;
using System.Text;
using System.Timers;
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

    public IrcClient(ILogger<IrcClient> logger)
    {
        _logger = logger;
        _pingInterval.Elapsed += PingIntervalOnElapsed;
        Channels.CollectionChanged += UpdateJoinedChannels;
    }

    public void Init(IIrcPoolManager ircPoolManager)
    {
        _ircPoolManager = ircPoolManager;
        _thread = new Thread(ThreadRun);
        _thread.Start();
        _logger.LogInformation("IrcClient Started");
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
            _logger.LogInformation("No PONG received for {Interval} s. Reconnecting...",
                _pingInterval.Interval / 1000);
            _pingInterval.Stop();
            await Disconnect();
            _awaitingPing = false;
        }
        else
        {
            if (_streamWriter != null)
            {
                await _streamWriter.WriteLineAsync("PING");
                await _streamWriter.FlushAsync();
            }

            _awaitingPing = true;
        }
    }

    private async Task Connect(CancellationToken stoppingToken)
    {
        Console.WriteLine($"Connecting to {Server}:{Port}");
        try
        {
            _tcpClient?.Close();
            _actualChannels.Clear();

            while (!_ircPoolManager.AuthenticateBucket.TakeTicket())
            {
                await Task.Delay(100, stoppingToken);
            }

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
                catch (OperationCanceledException)
                {
                    continue;
                }

                // No data = connection dead
                if (string.IsNullOrEmpty(line)) continue;
                IrcMessage? ircMessage = IrcParser.Parse(line);
                if (ircMessage == null) continue;

                await HandleIrcCommand(ircMessage, stoppingToken);
            }

            await _streamWriter.DisposeAsync();
        }
        catch (SocketException e)
        {
            // ignored
            _logger.LogError("{E}", e.ToString());
        }
        catch (IOException e)
        {
            // ignored
            _logger.LogError("{E}", e.ToString());
        }
        catch (Exception e)
        {
            _logger.LogError("{E}", e.ToString());
        }

        if (_streamWriter != null)
            await _streamWriter.DisposeAsync();
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

    private async Task HandleIrcCommand(IrcMessage ircMessage, CancellationToken stoppingToken)
    {
        switch (ircMessage.IrcCommand)
        {
            case IrcCommands.RplWelcome:
#pragma warning disable 4014
                Task.Delay(4000, stoppingToken).ContinueWith(_ =>
                {
                    _fullyConnected = true;
                    return UpdateJoinedChannels();
                }, stoppingToken);
#pragma warning restore 4014
                break;
            case IrcCommands.Ping:
                if (_streamWriter == null)
                    return;
                await _streamWriter.WriteLineAsync("PONG");
                await _streamWriter.FlushAsync();

                return;
            case IrcCommands.Pong:
                _awaitingPing = false;
                return;
            case IrcCommands.Reconnect:
                // automatic restart by checking _tcpClient.Connected in the while loop
                _tcpClient?.Close();
                return;
            case IrcCommands.Notice
                when ircMessage.IrcParameters[1].Contains("Login authentication failed"):

                _logger.LogWarning("Auth failed");
                await _ircPoolManager.RefreshAuth();
                return;
            case IrcCommands.GlobalUserstate:
                return;
            case IrcCommands.Userstate:
                return;
            case IrcCommands.Roomstate:
                //_logger.LogInformation("Roomstate: {RawSource}", ircMessage.RawSource);
                return;
            case IrcCommands.Join:
                //_logger.LogInformation("Join: {RawSource}", ircMessage.IrcParameters[0][1..]);
                string channelName = ircMessage.IrcParameters[0][1..];
                if (!_actualChannels.Contains(channelName))
                    _actualChannels.Add(channelName);
                return;
            case IrcCommands.Part:
                //_logger.LogInformation("Join: {RawSource}", ircMessage.RawSource);
                _actualChannels.Remove(ircMessage.IrcParameters[0][1..]);
                return;
            case IrcCommands.PrivMsg:
                await _ircPoolManager.NewPrivMsg(ircMessage);
                return;
            default:
                //_logger.LogInformation("{Command}—{RawSource}", ircMessage.IrcCommand, ircMessage.RawSource);
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

    private async void UpdateJoinedChannels(object? sender, NotifyCollectionChangedEventArgs args)
    {
        await UpdateJoinedChannels();
    }

    private async Task UpdateJoinedChannels()
    {
        if (_tcpClient is { Connected: false } || !_fullyConnected || _currentlyUpdatingChannels) return;

        _currentlyUpdatingChannels = true;

        // Do {} while () as we always want the body to run at least once!
        do
        {
            await WaitForLastChannelFlushed();
        } while (await PartExcessive() || await JoinMissing());

        if (_actualChannels.Except(Channels).Any())
            _logger.LogWarning("Still has channels to part: {Count}", _actualChannels.Except(Channels).Count());

        if (Channels.Except(_actualChannels).Any())
            _logger.LogWarning("Still has channels to join: {Count}", Channels.Except(_actualChannels).Count());

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
            _logger.LogInformation("{Msg}", msg);
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
        while (channelsToChange.Any() && _ircPoolManager.JoinBucket.TakeTicket())
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
            _logger.LogInformation("{Msg}", msg);
            if (_streamWriter != null)
            {
                await _streamWriter.WriteLineAsync(msg);
                await _streamWriter.FlushAsync();
            }

            _lastChannelChangeFlushed = DateTime.Now;
            hasChanged = true;
        }

        while (!_ircPoolManager.JoinBucket.TicketAvailable && channelsToChange.Any())
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
