using System.Text.Json.Serialization;

namespace TwitchIrcHub.IrcBot.PubSub.DataTypes.parsed.Whispers;

public class PubSubWhisperMessage
{
    [JsonPropertyName("message_id")]
    public string MessageId { get; init; } = null!;

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("thread_id")]
    public string ThreadId { get; init; } = null!;

    [JsonPropertyName("body")]
    public string Body { get; init; } = null!;

    [JsonPropertyName("sent_ts")]
    public int SentTs { get; init; }

    [JsonPropertyName("from_id")]
    public int FromId { get; init; }

    [JsonPropertyName("tags")]
    public PubSubWhisperMessageTags Tags { get; init; } = null!;

    [JsonPropertyName("recipient")]
    public PubSubWhisperMessageRecipient Recipient { get; init; } = null!;
}
