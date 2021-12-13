using System.Text.Json.Serialization;

namespace TwitchIrcHub.IrcBot.PubSub.DataTypes.parsed.Whispers;

public class PubSubWhisperThread
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;

    [JsonPropertyName("last_read")]
    public int LastRead { get; init; }

    [JsonPropertyName("archived")]
    public bool Archived { get; init; }

    [JsonPropertyName("muted")]
    public bool Muted { get; init; }

    [JsonPropertyName("spam_info")]
    public PubSubWhisperThreadSpamInfo SpamInfo { get; init; } = null!;

    [JsonPropertyName("whitelisted_until")]
    public DateTime WhitelistedUntil { get; init; }
}
