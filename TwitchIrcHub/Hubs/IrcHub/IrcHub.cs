using Microsoft.AspNetCore.SignalR;
using TwitchIrcHub.BackgroundServices;
using TwitchIrcHub.IrcBot.Bot;
using TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;
using TwitchIrcHub.Model;

namespace TwitchIrcHub.Hubs.IrcHub;

public class IrcHub : Hub<IIrcHub>
{
    private readonly ILogger<IrcHub> _logger;
    private readonly IrcHubDbContext _ircHubDbContext;
    public static readonly Dictionary<string, string> ConnectedClients = new();

    public IrcHub(ILogger<IrcHub> logger, IrcHubDbContext ircHubDbContext)
    {
        _logger = logger;
        _ircHubDbContext = ircHubDbContext;
    }

    public override async Task<Task> OnConnectedAsync()
    {
        if (string.IsNullOrEmpty(Context.UserIdentifier))
        {
            Console.WriteLine($"--> Connection failed: {Context.ConnectionId} (AppId: {Context.UserIdentifier})");
            return base.OnConnectedAsync();
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier);;
        ConnectedClients.Add(Context.ConnectionId, Context.UserIdentifier);
        Console.WriteLine($"--> Connection Opened: {Context.ConnectionId} (AppId: {Context.UserIdentifier})");
        await Clients.Client(Context.ConnectionId).ConnId(Context.ConnectionId);
        // TODO: Send all known UserState and GlobalUserState to client.

        if (!int.TryParse(Context.UserIdentifier, out int appId))
        {
            Console.WriteLine($"--> Connection failed: {Context.ConnectionId} (AppId: {Context.UserIdentifier})");
            return base.OnConnectedAsync();
        }

        // Get all roomId's for each botUserId matching the appId
        Dictionary<int, List<int>> channelsPerId = _ircHubDbContext.Connections
            .Where(connection => connection.RegisteredAppId == appId)
            .ToList()
            .GroupBy(connection => connection.BotUserId)
            .ToDictionary(
                grouping => grouping.Key,
                grouping => grouping.Select(connection => connection.RoomId).ToList()
            );

        List<Task> sendTasks = new();

        foreach ((int botUserId, List<int>? roomIds) in channelsPerId)
        {
            IBotInstance? botInstance = BotManager.GetBotInstance(botUserId);
            if (botInstance == null)
                continue;

            // GlobalUserState
            IrcGlobalUserState? globalUserState = botInstance.GetGlobalUserState();
            if (globalUserState != null)
                sendTasks.Add(Clients.Client(Context.ConnectionId).NewIrcGlobalUserState(globalUserState));

            // normal UserStates
            List<IrcUserState> userStates = await botInstance.GetUserStatesForChannels(roomIds);
            sendTasks.AddRange(userStates.Select(Clients.Client(Context.ConnectionId).NewIrcUserState));
        }

        await Task.WhenAll(sendTasks.ToArray());

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"--> Connection Closed: {Context.ConnectionId} (AppId: {Context.UserIdentifier})");
        ConnectedClients.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
