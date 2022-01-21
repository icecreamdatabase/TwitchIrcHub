using Microsoft.AspNetCore.SignalR;
using TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

namespace TwitchIrcHub.Hubs.IrcHub;

public static class IrcHubToClients
{
    public static async Task SendNewIrcClearChat(this IHubContext<IrcHub, IIrcHub> context, 
        IEnumerable<int> appIds, IrcClearChat ircClearChat)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcClearChat(ircClearChat);
    }

    public static async Task SendNewIrcClearMsg(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, IrcClearMsg ircClearMsg)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcClearMsg(ircClearMsg);
    }

    public static async Task SendNewIrcGlobalUserState(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, IrcGlobalUserState ircGlobalUserState)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcGlobalUserState(ircGlobalUserState);
    }

    public static async Task SendNewIrcHostTarget(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, IrcHostTarget ircHostTarget)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcHostTarget(ircHostTarget);
    }

    public static async Task SendNewIrcNotice(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, IrcNotice ircNotice)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcNotice(ircNotice);
    }

    public static async Task SendNewIrcPrivMsg(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, IrcPrivMsg ircPrivMsg)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcPrivMsg(ircPrivMsg);
    }

    public static async Task SendNewIrcRoomState(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, IrcRoomState ircRoomState)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcRoomState(ircRoomState);
    }

    public static async Task SendNewIrcUserNotice(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, IrcUserNotice ircUserNotice)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcUserNotice(ircUserNotice);
    }

    public static async Task SendNewIrcUserState(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, IrcUserState ircUserState)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcUserState(ircUserState);
    }
}
