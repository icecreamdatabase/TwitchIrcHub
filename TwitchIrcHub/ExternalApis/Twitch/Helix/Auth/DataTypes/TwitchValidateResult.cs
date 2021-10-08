using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TwitchIrcHub.ExternalApis.Twitch.Helix.Auth.DataTypes;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class TwitchValidateResult
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [JsonPropertyName("login")]
    public string Login { get; set; }

    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}