using System.Text.Json;
using System.Text.Json.Serialization;

namespace TwitchIrcHub.Helper;

public static class GlobalStatics
{
    public static readonly JsonSerializerOptions JsonIndentAndIgnoreNullValues = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static readonly JsonSerializerOptions JsonIgnoreNullValues = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
