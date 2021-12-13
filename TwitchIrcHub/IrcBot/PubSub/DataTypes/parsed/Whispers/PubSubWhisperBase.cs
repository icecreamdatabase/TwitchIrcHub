using System.Text.Json.Serialization;

namespace TwitchIrcHub.IrcBot.PubSub.DataTypes.parsed.Whispers;

public class PubSubWhisperBase<T> where T : class
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = null!;

    [JsonPropertyName("data")]
    public string Data { get; init; } = null!;

    [JsonPropertyName("data_object")]
    public T DataObject { get; init; } = null!;
}
