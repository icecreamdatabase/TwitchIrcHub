using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwitchIrcHub.Authentication.Policies;
using TwitchIrcHub.BackgroundServices;
using TwitchIrcHub.IrcBot.Bot;

namespace TwitchIrcHub.Controllers.AuthController;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = Policies.IsRegisteredApp)]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<AuthDataView>> Get([FromQuery] int? botUserId, [FromQuery] bool? forceVerify)
    {
        if (botUserId == null)
            return BadRequest();
        IBotInstance? bot = BotManager.GetBotInstance(botUserId.Value);
        if (bot == null)
            return NotFound();

        if (forceVerify.HasValue && forceVerify.Value)
            await bot.BotInstanceData.ValidateAccessToken();
        
        return Ok(new AuthDataView(bot.BotInstanceData));
    }
}
