using System.Text.Json;
using TwitchIrcHub.Helper;
using TwitchIrcHub.IrcBot.Bot;
using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.PubSub.DataTypes;
using TwitchIrcHub.IrcBot.PubSub.DataTypes.parsed.Whispers;
using TwitchIrcHub.IrcBot.PubSub.PubSubClient;

namespace TwitchIrcHub.IrcBot.PubSub.PubSubPoolManager;

public class PubSubPoolManager : IPubSubPoolManager
{
    private readonly ILogger<PubSubPoolManager> _logger;
    private readonly IFactory<IPubSubClient> _pubSubClientFactory;
    private BotInstance _botInstance = null!;
    private readonly List<IPubSubClient> _pubSubClients = new();
    private const int MaxTopicsPerPubSubClient = 50;
    public readonly List<string> Topics = new();

    public string BotOauth => _botInstance.BotInstanceData.AccessToken;

    public PubSubPoolManager(ILogger<PubSubPoolManager> logger, IFactory<IPubSubClient> pubSubClientFactory)
    {
        _logger = logger;
        _pubSubClientFactory = pubSubClientFactory;
    }

    public Task Init(BotInstance botInstance)
    {
        _botInstance = botInstance;
        //IPubSubClient pubSubClient = _pubSubClientFactory.Create();
        //pubSubClient.Init(this);
        //_pubSubClients.Add(pubSubClient);
        Topics.Add($"whispers.{_botInstance.BotInstanceData.UserId}");
        UpdateTopics();
        return Task.CompletedTask;
    }

    public Task IntervalPing()
    {
        UpdateTopics();
        return Task.CompletedTask;
    }

    private void UpdateTopics()
    {
        Unsubscribe(_pubSubClients.SelectMany(client => client.Topics).Except(Topics).ToList());
        Subscribe(Topics.Except(_pubSubClients.SelectMany(client => client.Topics)).ToList());
    }

    private void Subscribe(List<string> topics)
    {
        if (topics.Count == 0) return;

        foreach (IPubSubClient ircClient in _pubSubClients
                     .Where(ircClient => ircClient.Topics.Count < MaxTopicsPerPubSubClient)
                )
        {
            int freeSlots = MaxTopicsPerPubSubClient - ircClient.Topics.Count;
            List<string> newTopics = topics.Take(freeSlots).ToList();
            topics.RemoveRange(0, freeSlots);
            ircClient.Topics.AddRange(newTopics);
        }

        // Need new PubSubClient
        while (topics.Count > 0)
        {
            IPubSubClient ircClient = _pubSubClientFactory.Create();
            ircClient.Init(this);
            _pubSubClients.Add(ircClient);

            List<string> newTopics = topics.Take(Math.Min(topics.Count, MaxTopicsPerPubSubClient)).ToList();
            topics.RemoveRange(0, Math.Min(topics.Count, MaxTopicsPerPubSubClient));
            ircClient.Topics.AddRange(newTopics);
        }
    }

    private void Unsubscribe(IReadOnlyCollection<string> topics)
    {
        if (topics.Count == 0) return;

        foreach (string topic in topics)
            GetPubSubClientOfChannel(topic)?.Topics.Remove(topic);
    }

    private IPubSubClient? GetPubSubClientOfChannel(string topic)
    {
        return _pubSubClients.FirstOrDefault(client => client.Topics.Contains(topic));
    }

    public async Task NewIncomingPubSubMessage(PubSubIncomingMessage pubSubIncomingMessage)
    {
        if (pubSubIncomingMessage.Data == null)
            return;

        string[] splitTopic = pubSubIncomingMessage.Data.Topic.Split('.');
        string dataMessage = pubSubIncomingMessage.Data.Message;

        switch (splitTopic[0])
        {
            case PubSubTopics.Whispers:
            {
                string botUserId = splitTopic[1];
                HandleIncomingWhispers(dataMessage, botUserId);
                break;
            }
        }
    }

    private static void HandleIncomingWhispers(string dataMessage, string botUserId)
    {
        PubSubWhisperBase<object>? parsed =
            JsonSerializer.Deserialize<PubSubWhisperBase<object>>(dataMessage, GlobalStatics.JsonCaseInsensitive);

        if (parsed == null)
            return;

        switch (parsed.Type)
        {
            case PubSubIncomingWhisperType.Thread:
                PubSubWhisperBase<PubSubWhisperThread>? parsedThread =
                    JsonSerializer.Deserialize<PubSubWhisperBase<PubSubWhisperThread>>(dataMessage,
                        GlobalStatics.JsonCaseInsensitive);
                break;
            case PubSubIncomingWhisperType.WhisperSent:
                PubSubWhisperBase<PubSubWhisperMessage>? parsedSent =
                    JsonSerializer.Deserialize<PubSubWhisperBase<PubSubWhisperMessage>>(dataMessage,
                        GlobalStatics.JsonCaseInsensitive);
                break;
            case PubSubIncomingWhisperType.WhisperReceived:
                PubSubWhisperBase<PubSubWhisperMessage>? parsedReceived =
                    JsonSerializer.Deserialize<PubSubWhisperBase<PubSubWhisperMessage>>(dataMessage,
                        GlobalStatics.JsonCaseInsensitive);
                break;
            default:
                break;
        }
    }

    public void RemoveClient(IPubSubClient pubSubClient)
    {
        _pubSubClients.Remove(pubSubClient);
    }
}

internal static class PubSubTopics
{
    public const string ChannelBitsEventsV1 = "channel-bits-events-v1";
    public const string ChannelBitsEventsV2 = "channel-bits-events-v2";
    public const string ChannelBitsBadgeUnlocks = "channel-bits-badge-unlocks";
    public const string ChannelPointsChannelV1 = "channel-points-channel-v1";
    public const string ChannelSubscribeEventsV1 = "channel-subscribe-events-v1";
    public const string ChatModeratorActions = "chat_moderator_actions";
    public const string AutoModQueue = "automod-queue";
    public const string UserModerationNotifications = "user-moderation-notifications";
    public const string Whispers = "whispers";
}
