using Microsoft.AspNetCore.SignalR;
using TwitchIrcHub.IrcBot.Irc.DataTypes.Parsed;

namespace TwitchIrcHub.Hubs.IrcHub;

public class IrcHub : Hub<IIrcHub>
{
    public static readonly Dictionary<string, string> ConnectedClients = new();

    public override async Task<Task> OnConnectedAsync()
    {
        if (!string.IsNullOrEmpty(Context.UserIdentifier))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier);
            ConnectedClients.Add(Context.ConnectionId, Context.UserIdentifier);
            Console.WriteLine($"--> Connection Opened: {Context.ConnectionId} (AppId: {Context.UserIdentifier})");
            await Clients.Client(Context.ConnectionId).ConnId(Context.ConnectionId);
        }
        else
        {
            Console.WriteLine($"--> Connection failed: {Context.ConnectionId} (AppId: {Context.UserIdentifier})");
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"--> Connection Closed: {Context.ConnectionId} (AppId: {Context.UserIdentifier})");
        ConnectedClients.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public static async Task NewIrcPrivMsg(IHubContext<IrcHub, IIrcHub> context, IrcPrivMsg ircPrivMsg,
        IEnumerable<int> appIds)
    {
        foreach (int appId in appIds)
            await context.Clients.Group(appId.ToString()).NewIrcPrivMsg(ircPrivMsg);
    }
}
