using Microsoft.AspNetCore.SignalR;
using TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

namespace TwitchIrcHub.Hubs.IrcHub;

public static class IrcHubToClients
{
    public static async Task NewIrcClearChat(IHubContext<IrcHub, IIrcHub> context, IrcClearChat ircClearChat,
        List<int> appIds)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcClearChat(ircClearChat);
    }

    public static async Task NewIrcClearMsg(IHubContext<IrcHub, IIrcHub> context, IrcClearMsg ircClearMsg,
        List<int> appIds)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcClearMsg(ircClearMsg);
    }

    public static async Task NewIrcGlobalUserState(IHubContext<IrcHub, IIrcHub> context,
        IrcGlobalUserState ircGlobalUserState, List<int> appIds)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcGlobalUserState(ircGlobalUserState);
    }

    public static async Task NewIrcHostTarget(IHubContext<IrcHub, IIrcHub> context, IrcHostTarget ircHostTarget,
        List<int> appIds)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcHostTarget(ircHostTarget);
    }

    public static async Task NewIrcNotice(IHubContext<IrcHub, IIrcHub> context, IrcNotice ircNotice,
        List<int> appIds)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcNotice(ircNotice);
    }

    public static async Task NewIrcPrivMsg(IHubContext<IrcHub, IIrcHub> context, IrcPrivMsg ircPrivMsg,
        IEnumerable<int> appIds)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcPrivMsg(ircPrivMsg);
    }

    public static async Task NewIrcRoomState(IHubContext<IrcHub, IIrcHub> context, IrcRoomState ircRoomState,
        List<int> appIds)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcRoomState(ircRoomState);
    }

    public static async Task NewIrcUserNotice(IHubContext<IrcHub, IIrcHub> context, IrcUserNotice ircUserNotice,
        List<int> appIds)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcUserNotice(ircUserNotice);
    }

    public static async Task NewIrcUserState(IHubContext<IrcHub, IIrcHub> context, IrcUserState ircUserState,
        List<int> appIds)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcUserState(ircUserState);
    }
}
