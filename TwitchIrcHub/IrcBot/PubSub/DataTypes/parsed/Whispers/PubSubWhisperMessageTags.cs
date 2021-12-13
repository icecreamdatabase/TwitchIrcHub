using System.Text.Json.Serialization;

namespace TwitchIrcHub.IrcBot.PubSub.DataTypes.parsed.Whispers;

public class PubSubWhisperMessageTags
{
    [JsonPropertyName("login")]
    public string Login { get; init; } = null!;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; init; } = null!;

    [JsonPropertyName("color")]
    public string? Color { get; init; } = null!;

    [JsonPropertyName("emotes")]
    public PubSubWhisperMessageEmotes[] Emotes { get; init; } = Array.Empty<PubSubWhisperMessageEmotes>();

    [JsonPropertyName("badges")]
    public PubSubWhisperMessageBadges[] Badges { get; init; } = Array.Empty<PubSubWhisperMessageBadges>();
}
