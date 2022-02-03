using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwitchIrcHub.Authentication.Policies;
using TwitchIrcHub.ExternalApis.Twitch.Helix.Users;
using TwitchIrcHub.Model;
using TwitchIrcHub.Model.Schema;

namespace TwitchIrcHub.Controllers.ConnectionController;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = Policies.IsRegisteredApp)]
public class ConnectionController : ControllerBase
{
    private readonly ILogger<ConnectionController> _logger;

    public ConnectionController(ILogger<ConnectionController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ConnectionDataView>>> Get([FromServices] IrcHubDbContext ircHubDbContext)
    {
        string? appIdStr = HttpContext.User.Identity?.Name;

        if (!int.TryParse(appIdStr, out int appId))
            return BadRequest();
        List<ConnectionDataView> connectionDataViews = ircHubDbContext.Connections
            .Where(connection => connection.RegisteredAppId == appId)
            .ToList()
            .Select(connection => new ConnectionDataView(connection))
            .ToList();
        return Ok(connectionDataViews);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Put([FromBody] ConnectionRequestInput connectionRequestInput,
        [FromServices] IrcHubDbContext ircHubDbContext)
    {
        string? appIdStr = HttpContext.User.Identity?.Name;

        if (!int.TryParse(appIdStr, out int appId))
            return BadRequest();

        List<int> existingConnections = ircHubDbContext.Connections
            .Where(connection => connection.RegisteredAppId == appId &&
                                 connection.BotUserId == connectionRequestInput.BotUserId)
            .Select(connection => connection.RoomId)
            .ToList();

        List<int> existingChannels = ircHubDbContext.Channels.Select(channel => channel.RoomId).ToList();

        List<int> connectionsToAdd = connectionRequestInput.RoomIds.Except(existingConnections).ToList();
        List<int> channelsToAdd = connectionRequestInput.RoomIds.Except(existingChannels).ToList();

        Dictionary<string, string> idsToLogins = await TwitchUsers.IdsToLoginsWithCache(channelsToAdd);

        // Add required channels
        if (channelsToAdd.Count > 0)
            await ircHubDbContext.Channels.AddRangeAsync(
                channelsToAdd
                    .Where(roomId => idsToLogins.ContainsKey(roomId.ToString()))
                    .Select(roomId => new Channel
                    {
                        RoomId = roomId,
                        ChannelName = idsToLogins[roomId.ToString()]
                    })
            );

        // Add connections
        if (connectionsToAdd.Count > 0)
            await ircHubDbContext.Connections.AddRangeAsync(
                connectionsToAdd
                    .Where(roomId => idsToLogins.ContainsKey(roomId.ToString()))
                    .Select(roomId => new Connection
                    {
                        BotUserId = connectionRequestInput.BotUserId,
                        RoomId = roomId,
                        RegisteredAppId = appId
                    })
            );

        // Remove connections
        List<int> roomIdsToRemove = existingChannels.Except(connectionRequestInput.RoomIds).ToList();

        if (roomIdsToRemove.Count > 0)
            ircHubDbContext.Connections.RemoveRange(
                ircHubDbContext.Connections
                    .Where(connection => connection.RegisteredAppId == appId &&
                                         connection.BotUserId == connectionRequestInput.BotUserId &&
                                         roomIdsToRemove.Contains(connection.RoomId)
                    )
            );

        await ircHubDbContext.SaveChangesAsync();

        // Remove orphaned channels
        ircHubDbContext.Channels.RemoveRange(
            ircHubDbContext.Channels
                .Include(channel => channel.Connections)
                .Where(channel => channel.Connections.Count == 0)
        );

        await ircHubDbContext.SaveChangesAsync();
        return NoContent();
    }

    /*
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete([FromBody] ConnectionRequestInput connectionRequestInput,
        [FromServices] IrcHubDbContext ircHubDbContext)
    {
        string? appIdStr = HttpContext.User.Identity?.Name;

        if (!int.TryParse(appIdStr, out int appId))
            return BadRequest();

        // Remove connections
        ircHubDbContext.Connections.RemoveRange(
            ircHubDbContext.Connections
                .Where(connection => connection.RegisteredAppId == appId &&
                                     connection.BotUserId == connectionRequestInput.BotUserId &&
                                     connectionRequestInput.RoomIds.Contains(connection.RoomId)
                )
        );

        await ircHubDbContext.SaveChangesAsync();

        // Remove orphaned channels
        ircHubDbContext.Channels.RemoveRange(
            ircHubDbContext.Channels
                .Include(channel => channel.Connections)
                .Where(channel => channel.Connections.Count == 0)
        );

        await ircHubDbContext.SaveChangesAsync();
        return NoContent();
    }
    */
}
