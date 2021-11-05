using TwitchIrcHub.IrcBot.Irc.DataTypes.Parsed;

namespace TwitchIrcHub.Hubs.IrcHub;

public interface IIrcHub
{
    Task ConnId(string connectionId);

    Task NewIrcPrivMsg(IrcPrivMsg ircPrivMsg);
}
