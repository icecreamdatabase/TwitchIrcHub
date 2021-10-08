using TwitchIrcHub.Model;

namespace TwitchIrcHub.Controllers.AuthController.AuthData;

public class AuthData
{
    private protected const string BaseUrl = "https://id.twitch.tv/oauth2/authorize";
    private protected static string ClientId => BotDataAccess.ClientId;
    public const string RedirectUrlProduction = "https://botapi.icdb.dev";
    public const string RedirectUrlDevelopment = "https://botapitest.icdb.dev";
    public static string RedirectUrl { get; set; } = "";
}