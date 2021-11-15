using TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

namespace TwitchIrcHub.IrcBot.Irc.DataCache;

public class UserStateCache
{
    private readonly Dictionary<string, IrcUserState> _lastUserStatePerRoom = new();

    public void AddUserState(IrcUserState ircUserState)
    {
        _lastUserStatePerRoom[ircUserState.RoomName] = ircUserState;
    }

    public bool IsModInChannel(string roomName)
    {
        if (roomName.StartsWith('#'))
            roomName = roomName[1..];
        return _lastUserStatePerRoom.TryGetValue(roomName, out IrcUserState? ircUserState) &&
               (
                   ircUserState.IsMod ||
                   ircUserState.UserType == "mod" ||
                   ircUserState.Badges.ContainsKey("vip") ||
                   ircUserState.Badges.ContainsKey("moderator")
               );
    }
}
