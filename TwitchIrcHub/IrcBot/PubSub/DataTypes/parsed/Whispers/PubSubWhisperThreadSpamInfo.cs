using System.Text.Json.Serialization;

namespace TwitchIrcHub.IrcBot.PubSub.DataTypes.parsed.Whispers;

public class PubSubWhisperThreadSpamInfo
{
    [JsonPropertyName("likelihood")]
    public string Likelihood { get; init; } = null!;

    [JsonPropertyName("last_marked_not_spam")]
    public int LastMarkedNotSpam { get; init; }
}
