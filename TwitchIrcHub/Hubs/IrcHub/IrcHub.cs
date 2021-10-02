using Microsoft.AspNetCore.SignalR;

namespace TwitchIrcHub.Hubs.IrcHub;

public class IrcHub : Hub<IIrcHub>
{
        public override async Task<Task> OnConnectedAsync()
        {
            if (!string.IsNullOrEmpty(Context.UserIdentifier))
                await Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier);
            Console.WriteLine($"--> Connection Opened: {Context.ConnectionId} (roomId: {Context.UserIdentifier})");
            await Clients.Client(Context.ConnectionId).ConnId(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"--> Connection Closed: {Context.ConnectionId} (roomId: {Context.UserIdentifier})");
            return base.OnDisconnectedAsync(exception);
        }
}
