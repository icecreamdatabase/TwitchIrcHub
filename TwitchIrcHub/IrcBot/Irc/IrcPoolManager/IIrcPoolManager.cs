using TwitchIrcHub.IrcBot.Bot;
using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.DataTypes;
using TwitchIrcHub.IrcBot.Irc.IrcClient;

namespace TwitchIrcHub.IrcBot.Irc.IrcPoolManager;

public interface IIrcPoolManager
{
    public Task Init(BotInstance botInstance);
    public BasicBucket AuthenticateBucket { get; }
    public BasicBucket JoinBucket { get; }
    public string BotUsername { get; }
    public string BotOauth { get; }
    public Task ForceCheckAuth();
    public Task NewIrcMessage(IrcMessage ircMessage);
    public Task IntervalPing();
    public void RemoveReceiveClient(IIrcClient ircClient);
}