using TwitchIrcHub.IrcBot.Bot;
using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.DataTypes;

namespace TwitchIrcHub.IrcBot.Irc.IrcPoolManager;

public interface IIrcPoolManager
{
    public Task Init(BotInstance botInstance);
    public BasicBucket AuthenticateBucket { get; }
    public BasicBucket JoinBucket { get; }
    public string BotUsername { get; }
    public string BotOauth { get; }
    public Task RefreshAuth();
    public Task NewPrivMsg(IrcMessage ircMessage);
}