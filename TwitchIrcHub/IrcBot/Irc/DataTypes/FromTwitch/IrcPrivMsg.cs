using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class IrcPrivMsg
{
    public IrcMessage Raw { get; }

    /* --------------------------------------------------------------------------- */
    /* ------------------------------ Required tags ------------------------------ */
    /* --------------------------------------------------------------------------- */
    public Dictionary<string, int> BadgeInfo { get; }
    public Dictionary<string, int> Badges { get; }
    public string Color { get; }
    public string DisplayName { get; }
    public Dictionary<string, string[]> Emotes { get; }
    public bool FirstMsg { get; }
    public Dictionary<string, string[]> Flags { get; }
    public string Id { get; }
    public int RoomId { get; }
    public DateTime TmiSentTs { get; }
    public int UserId { get; }
    public bool IsMod { get; }
    public bool IsSubscriber { get; }
    public bool IsTurbo { get; }
    //TODO: UserType enum
    public string? UserType { get; }

    /* --------------------------------------------------------------------------- */
    /* --------------------- Non-tag but still required data --------------------- */
    /* --------------------------------------------------------------------------- */
    public string Message { get; }
    public bool IsAction { get; }
    public string RoomName { get; }
    public string UserName { get; }

    /* --------------------------------------------------------------------------- */
    /* ----------------------------- Conditional tags ---------------------------- */
    /* --------------------------------------------------------------------------- */
    public string? Bits { get; }
    public string? BitsImgUrl { get; }
    public string? ClientNonce { get; }
    public string? CrowdChantParentMsgId { get; }
    public string? CustomRewardId { get; }
    public bool? EmoteOnly { get; }
    public string? ReplyParentMsgId { get; }
    public string? ReplyParentUserId { get; }
    public string? ReplyParentUserLogin { get; }
    public string? ReplyParentDisplayName { get; }
    public string? ReplyParentMsgBody { get; }

    public IrcPrivMsg(IrcMessage ircMessage)
    {
        if (ircMessage.IrcCommand != IrcCommands.PrivMsg)
            throw new ArgumentOutOfRangeException(nameof(ircMessage), "Input is not a PrivMsg");

        Raw = ircMessage;

        // Try parsing all known tags.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        ircMessage.IrcMessageTags.TryGetValue("badge-info", out string? badgeInfo);
        ircMessage.IrcMessageTags.TryGetValue("badges", out string? badges);
        ircMessage.IrcMessageTags.TryGetValue("color", out string? color);
        ircMessage.IrcMessageTags.TryGetValue("display-name", out string? displayName);
        ircMessage.IrcMessageTags.TryGetValue("emotes", out string? emotes);
        ircMessage.IrcMessageTags.TryGetValue("first-msg", out string? firstMsg);
        ircMessage.IrcMessageTags.TryGetValue("flags", out string? flags);
        ircMessage.IrcMessageTags.TryGetValue("id", out string? id);
        ircMessage.IrcMessageTags.TryGetValue("mod", out string? mod);
        ircMessage.IrcMessageTags.TryGetValue("room-id", out string? roomId);
        ircMessage.IrcMessageTags.TryGetValue("subscriber", out string? subscriber);
        ircMessage.IrcMessageTags.TryGetValue("tmi-sent-ts", out string? tmiSentTs);
        ircMessage.IrcMessageTags.TryGetValue("turbo", out string? turbo);
        ircMessage.IrcMessageTags.TryGetValue("user-id", out string? userId);
        ircMessage.IrcMessageTags.TryGetValue("user-type", out string? userType);

        /* --------------------------------------------------------------------------- */
        /* ----------------------------- Conditional tags ---------------------------- */
        /* --------------------------------------------------------------------------- */
        ircMessage.IrcMessageTags.TryGetValue("bits", out string? bits);
        ircMessage.IrcMessageTags.TryGetValue("bits-img-url", out string? bitsImageUrl);
        ircMessage.IrcMessageTags.TryGetValue("client-nonce", out string? clientNonce);
        ircMessage.IrcMessageTags.TryGetValue("crowd-chant-parent-msg-id", out string? crowdChantParentMsgId);
        ircMessage.IrcMessageTags.TryGetValue("custom-reward-id", out string? customRewardId);
        ircMessage.IrcMessageTags.TryGetValue("emote-only", out string? emoteOnly);
        ircMessage.IrcMessageTags.TryGetValue("reply-parent-msg-id", out string? replyParentMsgId);
        ircMessage.IrcMessageTags.TryGetValue("reply-parent-user-id", out string? replyParentUserId);
        ircMessage.IrcMessageTags.TryGetValue("reply-parent-user-login", out string? replyParentUserLogin);
        ircMessage.IrcMessageTags.TryGetValue("reply-parent-display-name", out string? replyParentDisplayName);
        ircMessage.IrcMessageTags.TryGetValue("reply-parent-msg-body", out string? replyParentMsgBody);

        // Exceptions for nullable / missing tags that are not allowed to be missing.
        if (string.IsNullOrEmpty(id))
            throw new Exception($"PRIVMSG without valid id:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(roomId))
            throw new Exception($"PRIVMSG without valid roomId:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(displayName))
            throw new Exception($"PRIVMSG without valid displayName:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(userId))
            throw new Exception($"PRIVMSG without valid userId:\n{ircMessage.RawSource}");
        if (ircMessage.IrcParameters.Count < 2)
            throw new Exception($"PRIVMSG without valid roomName or message:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(ircMessage.IrcPrefix?.Username))
            throw new Exception($"PRIVMSG without valid userName:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(tmiSentTs))
            throw new Exception($"PRIVMSG without valid timestamp:\n{ircMessage.RawSource}");

        // Assign parsed tags to properties.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        BadgeInfo = IrcParseHelper.ParseBadgeData(badgeInfo);
        Badges = IrcParseHelper.ParseBadgeData(badges);
        Color = color ?? "";
        DisplayName = displayName;
        Emotes = IrcParseHelper.ParseEmoteData(emotes);
        FirstMsg = !string.IsNullOrEmpty(firstMsg) && firstMsg != "0";
        Flags = IrcParseHelper.ParseFlags(flags);
        Id = id;
        RoomId = int.Parse(roomId);
        TmiSentTs = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(tmiSentTs)).UtcDateTime;
        UserId = int.Parse(userId);
        IsMod = mod != null && mod != "0";
        IsSubscriber = subscriber != null && subscriber != "0";
        IsTurbo = turbo != null && turbo != "0";
        UserType = userType;

        /* --------------------------------------------------------------------------- */
        /* --------------------- Non-tag but still required data --------------------- */
        /* --------------------------------------------------------------------------- */
        Message = ircMessage.IrcParameters[1];
        if (Message.StartsWith("\u0001ACTION"))
        {
            Message = Message[8..^1];
            IsAction = true;
        }

        RoomName = ircMessage.IrcParameters[0][1..];
        UserName = ircMessage.IrcPrefix.Username;

        /* --------------------------------------------------------------------------- */
        /* ----------------------------- Conditional tags ---------------------------- */
        /* --------------------------------------------------------------------------- */
        Bits = bits;
        BitsImgUrl = bitsImageUrl;
        ClientNonce = clientNonce;
        CrowdChantParentMsgId = crowdChantParentMsgId;
        CustomRewardId = customRewardId;
        if (!string.IsNullOrEmpty(emoteOnly))
            EmoteOnly = emoteOnly != "0";
        ReplyParentMsgId = replyParentMsgId;
        ReplyParentUserId = replyParentUserId;
        ReplyParentUserLogin = replyParentUserLogin;
        ReplyParentDisplayName = replyParentDisplayName;
        ReplyParentMsgBody = replyParentMsgBody;
    }
}
