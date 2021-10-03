using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TwitchIrcHub.Model;
using TwitchIrcHub.Model.Schema;

namespace TwitchIrcHub.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    /// <summary>
    /// Will be written into <see cref="ClaimTypes"/>.<see cref="ClaimTypes.NameIdentifier"/>
    /// and is used all around the application for auth in regards to a channel
    /// </summary>
    private const string RoomIdQueryStringName = "roomId";

    private const string AccessTokenQueryStringName = "access_token";
    private const string ApiKeyHeaderName = "Authorization";
    private readonly IrcHubDbContext _ircHubDbContext;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IrcHubDbContext ircHubDbContext
    ) : base(options, logger, encoder, clock)
    {
        _ircHubDbContext = ircHubDbContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // get roomId query parameter. We cannot use custom headers for websockets. Using query parameters instead
        StringValues oAuthHeader = StringValues.Empty;
        if (
            !Context.Request.Query.TryGetValue(AccessTokenQueryStringName, out StringValues accessToken) &&
            !Request.Headers.TryGetValue(ApiKeyHeaderName, out oAuthHeader)
        )
        {
            // If neither accessToken, nor oAuthHeader are available
            // we can't authenticate at all --> No result
            return AuthenticateResult.NoResult();
        }

        // Try to use the OAuth header first. If that is empty use the access_token 
        string providedApiKey = oAuthHeader.FirstOrDefault() ?? string.Empty;
        if (string.IsNullOrEmpty(providedApiKey))
            providedApiKey = accessToken.FirstOrDefault() ?? string.Empty;

        // If we got an OAuth or Bearer token but it's empty we can't authenticate --> No result
        if (string.IsNullOrWhiteSpace(providedApiKey)) 
            return AuthenticateResult.NoResult();
            
        RegisteredApp? app = _ircHubDbContext.RegisteredApps.FirstOrDefault(app => app.Key == providedApiKey);
            
        // No app found --> No result
        if (app == null) 
            return AuthenticateResult.NoResult();
            
        List<Claim> claims = new()
        {
            new Claim(AuthClaims.AppId, app.Id.ToString()),
            new Claim(AuthClaims.AppName, app.AppName),
            new Claim(AuthClaims.AppKey, app.Key)
        };

        // Generate ticket based on the claims
        ClaimsIdentity identity = new(claims, ApiKeyAuthenticationOptions.AuthenticationType);
        List<ClaimsIdentity> identities = new() { identity };
        ClaimsPrincipal principal = new(identities);
        AuthenticationTicket ticket = new(principal, ApiKeyAuthenticationOptions.Scheme);

        return AuthenticateResult.Success(ticket);
    }
}