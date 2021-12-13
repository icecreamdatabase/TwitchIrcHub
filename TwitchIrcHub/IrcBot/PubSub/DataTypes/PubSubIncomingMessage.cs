using System.Text.Json.Serialization;

namespace TwitchIrcHub.IrcBot.PubSub.DataTypes;

public class PubSubIncomingMessage
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PubSubIncomingMessageType? Type { get; init; }

    [JsonPropertyName("nonce")]
    public string? Nonce { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("data")]
    public PubSubMessageData? Data { get; init; }
}

public class PubSubMessageData
{
    [JsonPropertyName("topic")]
    public string Topic { get; init; } = null!;

    [JsonPropertyName("message")]

    public string Message { get; init; } = null!;
}

public enum PubSubIncomingMessageType
{
    Pong,
    Reconnect,
    Response,
    Message
}
