using System.Collections.Specialized;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using TwitchIrcHub.Helper;
using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.PubSub.DataTypes;
using TwitchIrcHub.IrcBot.PubSub.PubSubPoolManager;
using Timer = System.Timers.Timer;

namespace TwitchIrcHub.IrcBot.PubSub.PubSubClient;

public class PubSubClient : IPubSubClient
{
    private readonly ILogger<PubSubClient> _logger;

    private const string Server = "wss://pubsub-edge.twitch.tv";

    private IPubSubPoolManager _pubSubPoolManager = null!;

    /* Ping */
    private bool _awaitingPing;
    private readonly Timer _pingInterval = new(20000);

    /* Reconnect */
    private static readonly Random ReconnectRandom = new();
    private const int ReconnectMaxJitter = 500;
    private const int ReconnectMultiplier = 2000;
    private const int ReconnectMaxMultiplier = 8;
    private int _reconnectionAttempts;

    /* PubSub */
    private ClientWebSocket? _clientWebSocket;

    /* Threading and shutdown */
    private Thread _thread = null!;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _autoRestart = true;

    /* Topics and subscribing */
    public BulkObservableCollection<string> Topics { get; } = new();
    private readonly List<string> _actualTopics = new();
    private bool _currentUpdatingTopics;

    public PubSubClient(ILogger<PubSubClient> logger)
    {
        _logger = logger;
        _pingInterval.Elapsed += PingIntervalOnElapsed;
        Topics.CollectionChanged += UpdateSubscribedTopics;
    }

    public void Init(IPubSubPoolManager pubSubPoolManager)
    {
        _pubSubPoolManager = pubSubPoolManager;
        _thread = new Thread(ThreadRun);
        _thread.Start();
        _logger.LogInformation("PubSubClient Started");
    }

    private async void ThreadRun()
    {
        CancellationToken cancellationToken = _cancellationTokenSource.Token;

        while (!cancellationToken.IsCancellationRequested && _autoRestart)
        {
            await Task.Delay(500, cancellationToken);
            await Connect(cancellationToken);
        }

        _clientWebSocket?.Dispose();
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
            _awaitingPing = true;
            _ = SendMessage(PubSubOutGoingMessage.PingMessage).ConfigureAwait(false);
        }
    }

    private async Task Connect(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Connecting to {Server}", Server);
        try
        {
            _clientWebSocket = new ClientWebSocket();
            await _clientWebSocket.ConnectAsync(new Uri(Server), stoppingToken);
            _ = Task.Delay(500, stoppingToken).ContinueWith(UpdateSubscribedTopics, stoppingToken);
            _pingInterval.Start();
            _reconnectionAttempts = 0;
            _logger.LogInformation("PubSub connected...");
            ArraySegment<byte> buffer = WebSocket.CreateClientBuffer(8192, 8192);
            while (_clientWebSocket.State != WebSocketState.Closed && !stoppingToken.IsCancellationRequested)
            {
                WebSocketReceiveResult receiveResult = await _clientWebSocket.ReceiveAsync(buffer, stoppingToken);
                if (stoppingToken.IsCancellationRequested)
                    break;

                if (_clientWebSocket.State == WebSocketState.CloseReceived &&
                    receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("Received close frame from PubSub server");
                    await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure,
                        "Acknowledge Close frame", CancellationToken.None);
                }

                if (_clientWebSocket.State == WebSocketState.Open &&
                    receiveResult.MessageType != WebSocketMessageType.Close &&
                    buffer.Array != null)
                {
                    _ = HandleIncomingMessage(Encoding.UTF8.GetString(buffer.Array, 0, receiveResult.Count))
                        .ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception e)
        {
            _logger.LogError("{E}", e.ToString());
        }

        if (!_cancellationTokenSource.IsCancellationRequested)
            await ReconnectDelayHandling();
    }

    private async Task HandleIncomingMessage(string rawMessage)
    {
        rawMessage = rawMessage.Trim();
        if (rawMessage.Length <= 1)
            return;
        PubSubIncomingMessage? parsed = JsonSerializer.Deserialize<PubSubIncomingMessage>(rawMessage);
        if (parsed == null)
            return;

        _logger.LogInformation("PubSub: {Message}", rawMessage.Trim());
        switch (parsed.Type)
        {
            case "PONG":
                _awaitingPing = false;
                return;
            case "RECONNECT":
                await Disconnect();
                return;
            case "RESPONSE":
                return;
            case "MESSAGE":
                _ = _pubSubPoolManager.NewIncomingPubSubMessage(parsed).ConfigureAwait(false);
                return;
            default:
                return;
        }
    }

    private async Task Disconnect()
    {
        if (_clientWebSocket is not { State: WebSocketState.Open })
            return;
        CancellationTokenSource timeout = new CancellationTokenSource(200);
        try
        {
            await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token);
            while (_clientWebSocket.State != WebSocketState.Closed && !timeout.Token.IsCancellationRequested)
                await Task.Delay(10, CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    public async Task Shutdown()
    {
        _autoRestart = false;
        await Disconnect();
        _cancellationTokenSource.Cancel();
    }

    public Task SendMessage(PubSubOutGoingMessage outGoingMessage)
    {
        return SendRawMessage(JsonSerializer.Serialize(outGoingMessage, GlobalStatics.JsonIgnoreNullValues));
    }

    private async Task SendRawMessage(string message)
    {
        _logger.LogInformation("Sending: {Message}", message);
        byte[] sendBytes = Encoding.UTF8.GetBytes(message);
        ArraySegment<byte> sendBuffer = new(sendBytes);
        if (_clientWebSocket is { State: WebSocketState.Open })
            await _clientWebSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task ReconnectDelayHandling()
    {
        int reconnectionDelay = 150;
        if (_reconnectionAttempts > 0)
        {
            int randomJitter = ReconnectRandom.Next(ReconnectMaxJitter + 1);
            reconnectionDelay = ReconnectMultiplier * _reconnectionAttempts - ReconnectMaxJitter + randomJitter;
        }

        await Task.Delay(reconnectionDelay, CancellationToken.None);

        _reconnectionAttempts = Math.Min(_reconnectionAttempts, ReconnectMaxMultiplier);
    }

    private async void UpdateSubscribedTopics(object? sender, NotifyCollectionChangedEventArgs e)
    {
        await UpdateSubscribedTopics();
    }

    private async void UpdateSubscribedTopics(Task obj)
    {
        await UpdateSubscribedTopics();
    }

    private async Task UpdateSubscribedTopics()
    {
        if (_clientWebSocket is not { State: WebSocketState.Open } || _currentUpdatingTopics)
            return;

        _currentUpdatingTopics = true;

        // Subscribe missing
        List<string> missingTopics = Topics.Except(_actualTopics).ToList();
        if (missingTopics.Count > 0)
        {
            await SendMessage(PubSubOutGoingMessage.GetSubscribe(missingTopics, _pubSubPoolManager.BotOauth));
            _actualTopics.AddRange(missingTopics);
        }

        // Unsubscribe redundant
        List<string> redundantTopics = _actualTopics.Except(Topics).ToList();
        if (redundantTopics.Count > 0)
        {
            await SendMessage(PubSubOutGoingMessage.GetUnsubscribe(redundantTopics, _pubSubPoolManager.BotOauth));
            redundantTopics.ForEach(topic => _actualTopics.Remove(topic));
        }

        if (Topics.Count == 0)
        {
            _pubSubPoolManager.RemoveClient(this);
            await Shutdown();
        }

        _currentUpdatingTopics = false;
    }
}
