using System.Text.Json.Serialization;

namespace TwitchIrcHub.ExternalApis.Discord.WebhookObjects;

public class WebhookEmbeds
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("color")]
    public int? Color { get; set; }

    [JsonPropertyName("footer")]
    public WebhookFooter? Footer { get; set; }

    [JsonPropertyName("author")]
    public WebhookAuthor? Author { get; set; }
}