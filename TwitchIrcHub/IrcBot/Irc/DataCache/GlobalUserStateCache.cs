using TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

namespace TwitchIrcHub.IrcBot.Irc.DataCache;

public class GlobalUserStateCache
{
    public IrcGlobalUserState? LastGlobalUserState { get; set; }
}
