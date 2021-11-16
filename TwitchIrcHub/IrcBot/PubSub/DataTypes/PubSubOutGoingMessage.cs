using System.Text.Json.Serialization;

namespace TwitchIrcHub.IrcBot.PubSub.DataTypes;

public class PubSubOutGoingMessage
{
    [JsonIgnore]
    public static readonly PubSubOutGoingMessage PingMessage = new()
    {
        Type = "PING"
    };

    public static PubSubOutGoingMessage GetSubscribe(List<string> topics, string authToken)
    {
        return new PubSubOutGoingMessage
        {
            Type = "LISTEN",
            Data = new PubSubRequestData
            {
                Topics = topics,
                AuthToken = authToken
            }
        };
    }
    public static PubSubOutGoingMessage GetUnsubscribe(List<string> topics, string authToken)
    {
        return new PubSubOutGoingMessage
        {
            Type = "UNLISTEN",
            Data = new PubSubRequestData
            {
                Topics = topics,
                AuthToken = authToken
            }
        };
    }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("nonce")]
    public string? Nonce { get; set; }
    [JsonPropertyName("data")]
    public PubSubRequestData? Data { get; set; }
}

public class PubSubRequestData
{
    [JsonPropertyName("topics")]
    public List<string> Topics { get; set; }
    [JsonPropertyName("auth_token")]
    public string AuthToken { get; set; }
}
