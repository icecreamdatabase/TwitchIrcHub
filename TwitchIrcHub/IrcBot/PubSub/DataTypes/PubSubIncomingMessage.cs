using System.Text.Json.Serialization;

namespace TwitchIrcHub.IrcBot.PubSub.DataTypes;

public class PubSubIncomingMessage
{

    [JsonPropertyName("type")]
    [JsonInclude]
    public string Type { get; init; } = null!;
    [JsonPropertyName("nonce")]
    [JsonInclude]
    public string? Nonce { get; init; }
    [JsonPropertyName("error")]
    [JsonInclude]
    public string? Error { get; init; }
    [JsonPropertyName("data")]
    [JsonInclude]
    public PubSubMessageData? Data { get; init; }
}

public class PubSubMessageData
{
    [JsonPropertyName("topic")]
    public string Topic { get; init; } = null!;
    [JsonPropertyName("message")]

    public string Message { get; init; } = null!;
}
