using System.Text.Json.Serialization;

namespace TwitchIrcHub.ExternalApis.Twitch.Helix;

public class HelixDataHolder<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; init; } = new();
}
