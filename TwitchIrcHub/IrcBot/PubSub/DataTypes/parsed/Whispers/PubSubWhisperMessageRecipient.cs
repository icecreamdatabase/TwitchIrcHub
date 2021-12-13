using System.Text.Json.Serialization;

namespace TwitchIrcHub.IrcBot.PubSub.DataTypes.parsed.Whispers;

public class PubSubWhisperMessageRecipient
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("username")]
    public string Username { get; init; } = null!;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; init; } = null!;

    [JsonPropertyName("color")]
    public string? Color { get; init; } = null!;
}
