using System.Text.Json.Serialization;

namespace TwitchIrcHub.IrcBot.PubSub.DataTypes.parsed.Whispers;

public class PubSubWhisperMessageBadges
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;

    [JsonPropertyName("version")]
    public string Version { get; init; } = "";
}
