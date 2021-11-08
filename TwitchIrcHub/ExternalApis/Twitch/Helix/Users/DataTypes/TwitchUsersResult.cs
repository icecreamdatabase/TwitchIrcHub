using System.Text.Json.Serialization;

namespace TwitchIrcHub.ExternalApis.Twitch.Helix.Users.DataTypes;

public class TwitchUsersResult
{
    [JsonPropertyName("broadcaster_type")]
    public string BroadCasterType { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = null!;

    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("login")]
    public string Login { get; set; } = null!;
    
    [JsonPropertyName("offline_image_url")]
    public string? OfflineImageUrl { get; set; }
    
    [JsonPropertyName("profile_image_url")]
    public string? ProfileImageUrl { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("view_count")]
    public int ViewCount { get; set; }
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = null!;
}
