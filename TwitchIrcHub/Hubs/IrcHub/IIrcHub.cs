using TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

namespace TwitchIrcHub.Hubs.IrcHub;

public interface IIrcHub
{
    Task ConnId(string connectionId);

    Task NewIrcClearMsg(int botUserId, IrcClearMsg ircClearMsg);
    Task NewIrcClearChat(int botUserId, IrcClearChat ircClearChat);
    Task NewIrcGlobalUserState(int botUserId, IrcGlobalUserState ircGlobalUserState);
    Task NewIrcHostTarget(int botUserId, IrcHostTarget ircHostTarget);
    Task NewIrcNotice(int botUserId, IrcNotice ircNotice);
    Task NewIrcPrivMsg(int botUserId, IrcPrivMsg ircPrivMsg);
    Task NewIrcRoomState(int botUserId, IrcRoomState ircRoomState);
    Task NewIrcUserNotice(int botUserId, IrcUserNotice ircUserNotice);
    Task NewIrcUserState(int botUserId, IrcUserState ircUserState);
}
