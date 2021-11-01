namespace TwitchIrcHub.Controllers.TwitchAppController.AuthData;

public class AuthDataAddBot : AuthData
{
    private const string ReponseType = "code";
    private const string RedirectUrlAppend = "/auth/register/bot";

    public static readonly string[] Scopes =
    {
        "bits:read",
        "moderation:read",
        "moderator:manage:automod",
        "channel:read:redemptions",
        "channel:manage:redemptions",
        "channel:moderate",
        "chat:edit",
        "chat:read",
        "user_blocks_edit",
        "user_blocks_read",
        "user_follows_edit",
        "user_subscriptions",
        "whispers:edit",
        "whispers:read"
    };

    public static readonly string FullUrl =
        $"{BaseUrl}" +
        $"?client_id={ClientId}" +
        $"&redirect_uri={RedirectUrl}{RedirectUrlAppend}" +
        $"&response_type={ReponseType}" +
        $"&scope={string.Join("+", Scopes)}" +
        $"&force_verify=true";
}