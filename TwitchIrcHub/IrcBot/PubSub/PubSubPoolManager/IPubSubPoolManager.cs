using TwitchIrcHub.IrcBot.Bot;
using TwitchIrcHub.IrcBot.PubSub.DataTypes;
using TwitchIrcHub.IrcBot.PubSub.PubSubClient;

namespace TwitchIrcHub.IrcBot.PubSub.PubSubPoolManager;

public interface IPubSubPoolManager
{
    public Task Init(BotInstance botInstance);
    public string BotOauth { get; }
    public Task IntervalPing();
    public Task NewIncomingPubSubMessage(PubSubIncomingMessage pubSubIncomingMessage);
    public void RemoveClient(IPubSubClient pubSubClient);
}
