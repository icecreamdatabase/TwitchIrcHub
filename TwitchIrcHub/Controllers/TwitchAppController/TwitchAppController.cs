using System.Net;
using Microsoft.AspNetCore.Mvc;
using TwitchIrcHub.Controllers.TwitchAppController.AuthData;
using TwitchIrcHub.ExternalApis.Twitch.Helix.Auth;
using TwitchIrcHub.ExternalApis.Twitch.Helix.Auth.DataTypes;
using TwitchIrcHub.Model;
using TwitchIrcHub.Model.Schema;

namespace TwitchIrcHub.Controllers.TwitchAppController;

[ApiController]
[Route("[controller]")]
public class TwitchAppController : ControllerBase
{
    private readonly ILogger<TwitchAppController> _logger;
    private readonly IrcHubDbContext _ircHubDbContext;

    public TwitchAppController(ILogger<TwitchAppController> logger, IrcHubDbContext ircHubDbContext)
    {
        _logger = logger;
        _ircHubDbContext = ircHubDbContext;
    }

    /// <summary>
    /// Redirects to the <c>oauth2/authorize</c> endpoint url required to add an account as a bot.
    /// The resulting code will then need to be sent to the <see cref="RegisterBot"/> endpoint.<br />
    /// <br />
    /// </summary>
    /// <returns></returns>
    /// <response code="302">Redirecting to uri</response>
    [HttpGet("Links/Bot")]
    [ProducesResponseType((int)HttpStatusCode.Redirect)]
    public ActionResult GetLinkBot()
    {
        return Redirect(AuthDataAddBot.FullUrl);
    }

    /// <summary>
    /// Register as a bot.
    /// TODO: Make this POST and use a separate page to do the POST call!
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <response code="204">Bot registered.</response>
    /// <response code="400">No code provided.</response>
    /// <response code="403">Invalid code or missing scopes.</response>
    /// <response code="404">Channel not found.</response>
    [HttpGet("Register/Bot")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> RegisterBot([FromQuery] AuthRegisterInput input)
    {
        if (!string.IsNullOrEmpty(input.Error) || string.IsNullOrEmpty(input.Code))
            return BadRequest(input.ErrorDescription);

        string clientId = BotDataAccess.ClientId;
        string clientSecret = BotDataAccess.ClientSecret;
        TwitchTokenResult? tokenResult =
            await TwitchAuthentication.GenerateAccessToken(clientId, clientSecret, input.Code);
        if (string.IsNullOrEmpty(tokenResult?.AccessToken))
            return Forbid();

        TwitchValidateResult? validateResult = await TwitchAuthentication.Validate(tokenResult.AccessToken);
        if (string.IsNullOrEmpty(validateResult?.UserId))
            return Forbid();

        bool hasAllRequiredScopes = AuthDataAddBot.Scopes
            .All(requiredScope => validateResult.Scopes
                .Select(s => s.ToLowerInvariant())
                .Contains(requiredScope.ToLowerInvariant())
            );
        if (!hasAllRequiredScopes)
            return Problem($"Missing scopes. Registering a bot requires: {string.Join(", ", AuthDataAddBot.Scopes)}",
                null, (int)HttpStatusCode.Forbidden);

        Bot? entity = _ircHubDbContext.Bots.FirstOrDefault(bot => bot.UserId == int.Parse(validateResult.UserId));

        if (entity is null)
        {
            entity = new Bot
            {
                UserId = int.Parse(validateResult.UserId),
                UserName = validateResult.Login
            };
            _ircHubDbContext.Bots.Add(entity);
        }
        else
        {
            if (!string.IsNullOrEmpty(entity.AccessToken))
                await TwitchAuthentication.Revoke(clientId, entity.AccessToken);
        }

        entity.RefreshToken = tokenResult.RefreshToken;
        entity.AccessToken = tokenResult.AccessToken;

        await _ircHubDbContext.SaveChangesAsync();

        return Ok("Bot register successful. You can now close this tab.");
    }
}
