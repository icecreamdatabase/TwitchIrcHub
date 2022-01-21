using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.Controllers.ConnectionController;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
public class ConnectionRequestInput
{
    public int BotUserId { get; init; }
    public List<int> RoomIds { get; init; } = new();
}
