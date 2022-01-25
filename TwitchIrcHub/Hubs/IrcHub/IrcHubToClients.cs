using Microsoft.AspNetCore.SignalR;
using TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

namespace TwitchIrcHub.Hubs.IrcHub;

public static class IrcHubToClients
{
    public static async Task SendNewIrcClearChat(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, int botUserId, IrcClearChat ircClearChat)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcClearChat(botUserId, ircClearChat);
    }

    public static async Task SendNewIrcClearMsg(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, int botUserId, IrcClearMsg ircClearMsg)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcClearMsg(botUserId, ircClearMsg);
    }

    public static async Task SendNewIrcGlobalUserState(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, int botUserId, IrcGlobalUserState ircGlobalUserState)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcGlobalUserState(botUserId, ircGlobalUserState);
    }

    public static async Task SendNewIrcHostTarget(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, int botUserId, IrcHostTarget ircHostTarget)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcHostTarget(botUserId, ircHostTarget);
    }

    public static async Task SendNewIrcNotice(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, int botUserId, IrcNotice ircNotice)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcNotice(botUserId, ircNotice);
    }

    public static async Task SendNewIrcPrivMsg(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, int botUserId, IrcPrivMsg ircPrivMsg)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcPrivMsg(botUserId, ircPrivMsg);
    }

    public static async Task SendNewIrcRoomState(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, int botUserId, IrcRoomState ircRoomState)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcRoomState(botUserId, ircRoomState);
    }

    public static async Task SendNewIrcUserNotice(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, int botUserId, IrcUserNotice ircUserNotice)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcUserNotice(botUserId, ircUserNotice);
    }

    public static async Task SendNewIrcUserState(this IHubContext<IrcHub, IIrcHub> context,
        IEnumerable<int> appIds, int botUserId, IrcUserState ircUserState)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcUserState(botUserId, ircUserState);
    }
}
