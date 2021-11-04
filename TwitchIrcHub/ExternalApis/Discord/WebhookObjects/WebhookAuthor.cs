using System.Text.Json.Serialization;

namespace TwitchIrcHub.ExternalApis.Discord.WebhookObjects;

public class WebhookAuthor
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }
}