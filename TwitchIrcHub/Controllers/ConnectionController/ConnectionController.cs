using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwitchIrcHub.Authentication.Policies;
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
    public async Task<ActionResult> Put([FromQuery] int? botUserId, [FromQuery] int? roomId,
        [FromServices] IrcHubDbContext ircHubDbContext)
    {
        if (botUserId == null || roomId == null)
            return BadRequest();

        string? appIdStr = HttpContext.User.Identity?.Name;

        if (!int.TryParse(appIdStr, out int appId))
            return BadRequest();

        bool connectionExists = ircHubDbContext.Connections.Any(connection => connection.RegisteredAppId == appId &&
                                                                              connection.BotUserId == botUserId &&
                                                                              connection.RoomId == roomId);
        if (connectionExists)
            return NoContent();

        ircHubDbContext.Connections.Add(new Connection
        {
            BotUserId = botUserId.Value,
            RoomId = roomId.Value,
            RegisteredAppId = appId
        });
        await ircHubDbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromQuery] int? botUserId, [FromQuery] int? roomId,
        [FromServices] IrcHubDbContext ircHubDbContext)
    {
        if (botUserId == null || roomId == null)
            return BadRequest();

        string? appIdStr = HttpContext.User.Identity?.Name;

        if (!int.TryParse(appIdStr, out int appId))
            return BadRequest();

        Connection? connection = ircHubDbContext.Connections.FirstOrDefault(connection =>
            connection.RegisteredAppId == appId &&
            connection.BotUserId == botUserId &&
            connection.RoomId == roomId
        );
        if (connection == null)
            return NotFound();

        ircHubDbContext.Connections.Remove(connection);
        await ircHubDbContext.SaveChangesAsync();

        return NoContent();
    }
}