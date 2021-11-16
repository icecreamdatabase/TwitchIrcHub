using TwitchIrcHub.IrcBot.Bot;
using TwitchIrcHub.IrcBot.Irc.DataCache;
using TwitchIrcHub.IrcBot.Irc.DataTypes;
using TwitchIrcHub.IrcBot.Irc.DataTypes.ToTwitch;
using TwitchIrcHub.IrcBot.Irc.IrcClient;

namespace TwitchIrcHub.IrcBot.Irc.IrcPoolManager;

public interface IIrcPoolManager
{
    public Task Init(BotInstance botInstance);
    public IrcBuckets IrcBuckets { get; }
    public UserStateCache UserStateCache { get; }
    public GlobalUserStateCache GlobalUserStateCache { get; }
    public string BotUsername { get; }
    public string BotOauth { get; }
    public Task ForceCheckAuth();
    public Task NewIrcMessage(IrcMessage ircMessage);
    public Task IntervalPing();
    public void RemoveReceiveClient(IIrcClient ircClient);
    public void SendMessage(PrivMsgToTwitch privMsg);
    public Task SendMessageNoQueue(PrivMsgToTwitch privMsgToTwitch);
}
