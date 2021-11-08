using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwitchIrcHub.Authentication.Policies;
using TwitchIrcHub.ExternalApis.Twitch.Helix.Users;
using TwitchIrcHub.ExternalApis.Twitch.Helix.Users.DataTypes;

namespace TwitchIrcHub.Controllers.IdLoginTranslationController;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = Policies.IsRegisteredApp)]
public class IdLoginTranslationController : ControllerBase
{
    private readonly ILogger<IdLoginTranslationController> _logger;

    public IdLoginTranslationController(ILogger<IdLoginTranslationController> logger)
    {
        _logger = logger;
    }

    [HttpGet("Users")]
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
        List<TwitchUsersResult>? users = await TwitchUsers.Users(ids);
        return Ok(users == null
            ? new Dictionary<string, string>()
            : users.ToDictionary(user => user.Id, user => user.Login)
        );
    }
    
    [HttpGet("LoginToId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, string>>> LoginToId(
        [FromQuery(Name = "login")] List<string> logins
    )
    {
        List<TwitchUsersResult>? users = await TwitchUsers.Users(logins: logins);
        return Ok(users == null
            ? new Dictionary<string, string>()
            : users.ToDictionary(user => user.Login, user => user.Id)
        );
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult Add([FromQuery] string id, [FromQuery] string login)
    {
        return NoContent();
    }
}
