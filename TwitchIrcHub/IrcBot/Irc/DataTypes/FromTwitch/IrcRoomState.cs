using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class IrcRoomState
{
    public IrcMessage Raw { get; }

    /* --------------------------------------------------------------------------- */
    /* ------------------------------ Required tags ------------------------------ */
    /* --------------------------------------------------------------------------- */
    public bool EmoteOnly { get; }
    public int FollowersOnly { get; }
    public bool R9K { get; }
    public bool Rituals { get; }
    public int RoomId { get; }
    public int Slow { get; }
    public bool SubsOnly { get; }

    /* --------------------------------------------------------------------------- */
    /* --------------------- Non-tag but still required data --------------------- */
    /* --------------------------------------------------------------------------- */
    public string RoomName { get; }


    public IrcRoomState(IrcMessage ircMessage)
    {
        if (ircMessage.IrcCommand != IrcCommands.RoomState)
            throw new ArgumentOutOfRangeException(nameof(ircMessage), "Input is not a RoomState");

        Raw = ircMessage;

        // Try parsing all known tags.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        ircMessage.IrcMessageTags.TryGetValue("emote-only", out string? emoteOnly);
        ircMessage.IrcMessageTags.TryGetValue("followers-only", out string? followersOnly);
        ircMessage.IrcMessageTags.TryGetValue("r9k", out string? r9K);
        ircMessage.IrcMessageTags.TryGetValue("rituals", out string? rituals);
        ircMessage.IrcMessageTags.TryGetValue("room-id", out string? roomId);
        ircMessage.IrcMessageTags.TryGetValue("slow", out string? slow);
        ircMessage.IrcMessageTags.TryGetValue("subs-only", out string? subsOnly);

        // Exceptions for nullable / missing tags that are not allowed to be missing.
        if (string.IsNullOrEmpty(roomId))
            throw new Exception($"ROOMSTATE without valid roomId:\n{ircMessage.RawSource}");
        if (ircMessage.IrcParameters.Count < 1)
            throw new Exception($"ROOMSTATE without valid roomName:\n{ircMessage.RawSource}");

        // Assign parsed tags to properties.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        EmoteOnly = !string.IsNullOrEmpty(emoteOnly) && emoteOnly != "0";
        FollowersOnly = string.IsNullOrEmpty(followersOnly) ? -1 : int.Parse(followersOnly);
        R9K = !string.IsNullOrEmpty(r9K) && r9K != "0";
        Rituals = !string.IsNullOrEmpty(rituals) && rituals != "0";
        RoomId = int.Parse(roomId);
        Slow = string.IsNullOrEmpty(slow) ? 0 : int.Parse(slow);
        SubsOnly = !string.IsNullOrEmpty(subsOnly) && subsOnly != "0";

        /* --------------------------------------------------------------------------- */
        /* --------------------- Non-tag but still required data --------------------- */
        /* --------------------------------------------------------------------------- */
        RoomName = ircMessage.IrcParameters[0][1..];
    }
}
