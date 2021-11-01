using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes.Parsed;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class IrcClearMsg
{
    public IrcMessage Raw { get; }

    /* --------------------------------------------------------------------------- */
    /* ------------------------------ Required tags ------------------------------ */
    /* --------------------------------------------------------------------------- */
    public string Login { get; }
    public int RoomId { get; }
    public string TargetMsgId { get; }
    public DateTime TmiSentTs { get; }

    /* --------------------------------------------------------------------------- */
    /* --------------------- Non-tag but still required data --------------------- */
    /* --------------------------------------------------------------------------- */
    public string Message { get; }
    public string RoomName { get; }

    public IrcClearMsg(IrcMessage ircMessage)
    {
        if (ircMessage.IrcCommand != IrcCommands.ClearMsg)
            throw new ArgumentOutOfRangeException(nameof(ircMessage), "Input is not a ClearMsg");

        Raw = ircMessage;

        // Try parsing all known tags.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        ircMessage.IrcMessageTags.TryGetValue("login", out string? login);
        ircMessage.IrcMessageTags.TryGetValue("room-id", out string? roomId);
        ircMessage.IrcMessageTags.TryGetValue("target-msg-id", out string? targetMsgId);
        ircMessage.IrcMessageTags.TryGetValue("tmi-sent-ts", out string? tmiSentTs);

        // Exceptions for nullable / missing tags that are not allowed to be missing.
        if (string.IsNullOrEmpty(login))
            throw new Exception($"CLEARMSG without valid login:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(roomId))
            throw new Exception($"CLEARMSG without valid roomId:\n{ircMessage.RawSource}");
        if (ircMessage.IrcParameters.Count < 2)
            throw new Exception($"CLEARMSG without valid roomName or message:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(targetMsgId))
            throw new Exception($"CLEARMSG without valid target-msg-id:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(tmiSentTs))
            throw new Exception($"CLEARMSG without valid timestamp:\n{ircMessage.RawSource}");

        // Assign parsed tags to properties.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        Login = login;
        RoomId = int.Parse(roomId);
        TargetMsgId = targetMsgId;
        TmiSentTs = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(tmiSentTs)).UtcDateTime;

        /* --------------------------------------------------------------------------- */
        /* --------------------- Non-tag but still required data --------------------- */
        /* --------------------------------------------------------------------------- */
        Message = ircMessage.IrcParameters[1];
        RoomName = ircMessage.IrcParameters[0][1..];
    }
}
