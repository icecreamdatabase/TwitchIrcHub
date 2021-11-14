using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class IrcClearChat
{
    public IrcMessage Raw { get; }

    /* --------------------------------------------------------------------------- */
    /* ------------------------------ Required tags ------------------------------ */
    /* --------------------------------------------------------------------------- */
    public int? BanDuration { get; }
    public int RoomId { get; }
    public string? TargetUserId { get; }
    public DateTime TmiSentTs { get; }

    /* --------------------------------------------------------------------------- */
    /* --------------------- Non-tag but still required data --------------------- */
    /* --------------------------------------------------------------------------- */
    public string RoomName { get; }
    public string? TargetUserName { get; }

    public IrcClearChat(IrcMessage ircMessage)
    {
        if (ircMessage.IrcCommand != IrcCommands.ClearChat)
            throw new ArgumentOutOfRangeException(nameof(ircMessage), "Input is not a ClearChat");

        Raw = ircMessage;

        // Try parsing all known tags.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        ircMessage.IrcMessageTags.TryGetValue("ban-duration", out string? banDuration);
        ircMessage.IrcMessageTags.TryGetValue("room-id", out string? roomId);
        ircMessage.IrcMessageTags.TryGetValue("target-user-id", out string? targetUserId);
        ircMessage.IrcMessageTags.TryGetValue("tmi-sent-ts", out string? tmiSentTs);

        // Exceptions for nullable / missing tags that are not allowed to be missing.
        if (string.IsNullOrEmpty(roomId))
            throw new Exception($"CLEARCHAT without valid roomId:\n{ircMessage.RawSource}");
        if (ircMessage.IrcParameters.Count < 1)
            throw new Exception($"CLEARCHAT without valid roomName:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(tmiSentTs))
            throw new Exception($"CLEARCHAT without valid timestamp:\n{ircMessage.RawSource}");

        // Assign parsed tags to properties.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        if (!string.IsNullOrEmpty(banDuration))
            BanDuration = int.Parse(banDuration);
        RoomId = int.Parse(roomId);
        if (!string.IsNullOrEmpty(targetUserId))
            TargetUserId = targetUserId;
        TmiSentTs = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(tmiSentTs)).UtcDateTime;

        /* --------------------------------------------------------------------------- */
        /* --------------------- Non-tag but still required data --------------------- */
        /* --------------------------------------------------------------------------- */
        if (ircMessage.IrcParameters.Count > 1 && !string.IsNullOrEmpty(ircMessage.IrcParameters[1]))
            TargetUserName = ircMessage.IrcParameters[1];
        RoomName = ircMessage.IrcParameters[0][1..];
    }
}
