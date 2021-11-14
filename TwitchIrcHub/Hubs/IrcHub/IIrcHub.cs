using TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

namespace TwitchIrcHub.Hubs.IrcHub;

public interface IIrcHub
{
    Task ConnId(string connectionId);

    Task NewIrcClearMsg(IrcClearMsg ircClearMsg);
    Task NewIrcClearChat(IrcClearChat ircClearChat);
    Task NewIrcGlobalUserState(IrcGlobalUserState ircGlobalUserState);
    Task NewIrcHostTarget(IrcHostTarget ircHostTarget);
    Task NewIrcNotice(IrcNotice ircNotice);
    Task NewIrcPrivMsg(IrcPrivMsg ircPrivMsg);
    Task NewIrcRoomState(IrcRoomState ircRoomState);
    Task NewIrcUserNotice(IrcUserNotice ircUserNotice);
    Task NewIrcUserState(IrcUserState ircUserState);
}
