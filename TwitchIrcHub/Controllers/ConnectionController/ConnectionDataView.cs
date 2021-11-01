using System.Diagnostics.CodeAnalysis;
using TwitchIrcHub.Model.Schema;

namespace TwitchIrcHub.Controllers.ConnectionController;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ConnectionDataView
{
    public int BotUserId { get; }
    public int RoomId { get; }
    public int RegisteredAppId { get; }

    public ConnectionDataView(Connection connection)
    {
        BotUserId = connection.BotUserId;
        RoomId = connection.RoomId;
        RegisteredAppId = connection.RegisteredAppId;
    }
}
