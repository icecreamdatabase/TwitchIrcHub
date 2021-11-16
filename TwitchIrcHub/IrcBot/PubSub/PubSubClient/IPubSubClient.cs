using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.PubSub.DataTypes;
using TwitchIrcHub.IrcBot.PubSub.PubSubPoolManager;

namespace TwitchIrcHub.IrcBot.PubSub.PubSubClient;

public interface IPubSubClient
{
    public void Init(IPubSubPoolManager pubSubPoolManager);
    public BulkObservableCollection<string> Topics { get; }
    public Task Shutdown();
    public Task SendMessage(PubSubOutGoingMessage outGoingMessage);
}
