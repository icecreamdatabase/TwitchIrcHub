using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwitchIrcHub.Authentication.Policies;
using TwitchIrcHub.ExternalApis.Twitch.Helix.Users;
using TwitchIrcHub.ExternalApis.Twitch.Helix.Users.DataTypes;

namespace TwitchIrcHub.Controllers.TwitchUsersController;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = Policies.IsRegisteredApp)]
public class TwitchUsersController : ControllerBase
{
    private readonly ILogger<TwitchUsersController> _logger;

    public TwitchUsersController(ILogger<TwitchUsersController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TwitchUsersResult>>> Users(
        [FromQuery(Name = "id")] List<string> ids,
        [FromQuery(Name = "login")] List<string> logins
    )
    {
        List<TwitchUsersResult>? users = await TwitchUsers.Users(ids, logins);
        return Ok(users ?? new List<TwitchUsersResult>());
    }

    [HttpGet("IdToLogin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, string>>> IdToLogin(
        [FromQuery(Name = "id")] List<string> ids
    )
    {
        return Ok(await TwitchUsers.IdsToLoginsWithCache(ids));
    }

    [HttpGet("LoginToId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, string>>> LoginToId(
        [FromQuery(Name = "login")] List<string> logins
    )
    {
        return Ok(await TwitchUsers.LoginsToIdsWithCache(logins));
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult Add([FromQuery] string id, [FromQuery] string login)
    {
        return NoContent();
    }
}
