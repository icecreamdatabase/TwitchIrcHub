using Microsoft.AspNetCore.Authentication;

namespace TwitchIrcHub.Authentication;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "API Key";
    public const string Scheme = DefaultScheme;
    public const string AuthenticationType = DefaultScheme;
}