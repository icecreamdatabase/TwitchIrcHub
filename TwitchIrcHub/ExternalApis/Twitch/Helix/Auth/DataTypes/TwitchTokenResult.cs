using System.Text.Json.Serialization;

namespace TwitchIrcHub.ExternalApis.Twitch.Helix.Auth.DataTypes;

public class TwitchTokenResult
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
}