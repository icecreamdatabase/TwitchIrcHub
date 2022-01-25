using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class IrcUserNotice
{
    public IrcMessage Raw { get; }

    /* --------------------------------------------------------------------------- */
    /* ------------------------------ Required tags ------------------------------ */
    /* --------------------------------------------------------------------------- */
    public Dictionary<string, string> BadgeInfo { get; }
    public Dictionary<string, string> Badges { get; }
    public string Color { get; }
    public string DisplayName { get; }
    public Dictionary<string, string[]> Emotes { get; }
    public string Id { get; }
    public string Login { get; }
    public string? Message { get; }
    public UserNoticeMessageId MessageId { get; }
    public int RoomId { get; }
    public string SystemMessage { get; }
    public DateTime TmiSentTs { get; }
    public int UserId { get; }

    /* --------------------------------------------------------------------------- */
    /* --------------------- Non-tag but still required data --------------------- */
    /* --------------------------------------------------------------------------- */
    public string RoomName { get; }
    public string? UserInput { get; }

    /* --------------------------------------------------------------------------- */
    /* ----------------------------- Conditional tags ---------------------------- */
    /* --------------------------------------------------------------------------- */
    public string? MsgParamCumulativeMonths { get; }
    public string? MsgParamDisplayName { get; }
    public string? MsgParamLogin { get; }
    public string? MsgParamMonths { get; }
    public string? MsgParamPromoGiftTotal { get; }
    public string? MsgParamPromoName { get; }
    public string? MsgParamRecipientDisplayName { get; }
    public string? MsgParamRecipientId { get; }
    public string? MsgParamRecipientUserName { get; }
    public string? MsgParamSenderLogin { get; }
    public string? MsgParamSenderName { get; }
    public string? MsgParamShouldShareStreak { get; }
    public string? MsgParamStreakMonths { get; }
    public string? MsgParamSubPlan { get; }
    public string? MsgParamSubPlanName { get; }
    public string? MsgParamViewerCount { get; }
    public UserNoticeRitualName? MsgParamRitualName { get; }
    public string? MsgParamThreshold { get; }
    public string? MsgParamGiftMonths { get; }

    public IrcUserNotice(IrcMessage ircMessage)
    {
        if (ircMessage.IrcCommand != IrcCommands.UserNotice)
            throw new ArgumentOutOfRangeException(nameof(ircMessage), "Input is not a UserNotice");

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
        ircMessage.IrcMessageTags.TryGetValue("id", out string? id);
        ircMessage.IrcMessageTags.TryGetValue("login", out string? login);
        ircMessage.IrcMessageTags.TryGetValue("message", out string? message);
        ircMessage.IrcMessageTags.TryGetValue("msg-id", out string? messageId);
        ircMessage.IrcMessageTags.TryGetValue("room-id", out string? roomId);
        ircMessage.IrcMessageTags.TryGetValue("system-msg", out string? systemMessage);
        ircMessage.IrcMessageTags.TryGetValue("tmi-sent-ts", out string? tmiSentTs);
        ircMessage.IrcMessageTags.TryGetValue("user-id", out string? userId);

        /* --------------------------------------------------------------------------- */
        /* ----------------------------- Conditional tags ---------------------------- */
        /* --------------------------------------------------------------------------- */
        ircMessage.IrcMessageTags.TryGetValue("msg-param-cumulative-months", out string? msgParamCumulativeMonths);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-displayName", out string? msgParamDisplayName);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-login", out string? msgParamLogin);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-months", out string? msgParamMonths);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-promo-gift-total", out string? msgParamPromoGiftTotal);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-promo-name", out string? msgParamPromoName);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-recipient-display-name", out string? msgParamRecipientDisplayName);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-recipient-id", out string? msgParamRecipientId);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-recipient-user-name", out string? msgParamRecipientUserName);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-sender-login", out string? msgParamSenderLogin);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-sender-name", out string? msgParamSenderName);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-should-share-streak", out string? msgParamShouldShareStreak);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-streak-months", out string? msgParamStreakMonths);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-sub-plan", out string? msgParamSubPlan);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-sub-plan-name", out string? msgParamSubPlanName);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-viewerCount", out string? msgParamViewerCount);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-ritual-name", out string? msgParamRitualName);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-threshold", out string? msgParamThreshold);
        ircMessage.IrcMessageTags.TryGetValue("msg-param-gift-months", out string? msgParamGiftMonths);

        // Exceptions for nullable / missing tags that are not allowed to be missing.
        if (string.IsNullOrEmpty(id))
            throw new Exception($"USERNOTICE without valid id:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(roomId))
            throw new Exception($"USERNOTICE without valid roomId:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(displayName))
            throw new Exception($"USERNOTICE without valid displayName:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(login))
            throw new Exception($"USERNOTICE without valid login:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(systemMessage))
            throw new Exception($"USERNOTICE without valid systemMessage:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(userId))
            throw new Exception($"USERNOTICE without valid userId:\n{ircMessage.RawSource}");
        if (ircMessage.IrcParameters.Count < 1)
            throw new Exception($"USERNOTICE without valid roomName:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(tmiSentTs))
            throw new Exception($"USERNOTICE without valid timestamp:\n{ircMessage.RawSource}");

        // Assign parsed tags to properties.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        BadgeInfo = IrcParseHelper.ParseBadgeData(badgeInfo);
        Badges = IrcParseHelper.ParseBadgeData(badges);
        Color = color ?? "";
        DisplayName = displayName;
        Emotes = IrcParseHelper.ParseEmoteData(emotes);
        Id = id;
        Login = login;
        Message = message; // TODO: is this ever used?
        MessageId = Enum.TryParse(
            messageId,
            true,
            out UserNoticeMessageId parsedMessageId
        )
            ? parsedMessageId
            : UserNoticeMessageId.ParsingError;
        RoomId = int.Parse(roomId);
        SystemMessage = systemMessage;
        TmiSentTs = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(tmiSentTs)).UtcDateTime;
        UserId = int.Parse(userId);

        /* --------------------------------------------------------------------------- */
        /* --------------------- Non-tag but still required data --------------------- */
        /* --------------------------------------------------------------------------- */
        RoomName = ircMessage.IrcParameters[0][1..];
        if (ircMessage.IrcParameters.Count > 1)
            UserInput = ircMessage.IrcParameters[1]; 

        /* --------------------------------------------------------------------------- */
        /* ----------------------------- Conditional tags ---------------------------- */
        /* --------------------------------------------------------------------------- */
        MsgParamCumulativeMonths = msgParamCumulativeMonths;
        MsgParamDisplayName = msgParamDisplayName;
        MsgParamLogin = msgParamLogin;
        MsgParamMonths = msgParamMonths;
        MsgParamPromoGiftTotal = msgParamPromoGiftTotal;
        MsgParamPromoName = msgParamPromoName;
        MsgParamRecipientDisplayName = msgParamRecipientDisplayName;
        MsgParamRecipientId = msgParamRecipientId;
        MsgParamRecipientUserName = msgParamRecipientUserName;
        MsgParamSenderLogin = msgParamSenderLogin;
        MsgParamSenderName = msgParamSenderName;
        MsgParamShouldShareStreak = msgParamShouldShareStreak;
        MsgParamStreakMonths = msgParamStreakMonths;
        MsgParamSubPlan = msgParamSubPlan;
        MsgParamSubPlanName = msgParamSubPlanName;
        MsgParamViewerCount = msgParamViewerCount;
        MsgParamRitualName = Enum.TryParse(
            msgParamRitualName?.Replace("_", ""),
            true,
            out UserNoticeRitualName parsedMsgParamRitualName
        )
            ? parsedMsgParamRitualName
            : UserNoticeRitualName.ParsingError;
        MsgParamThreshold = msgParamThreshold;
        MsgParamGiftMonths = msgParamGiftMonths;
    }
}

public enum UserNoticeMessageId
{
    ParsingError,
    Sub,
    Resub,
    SubGift,
    AnonSubGift,
    SubMysteryGift,
    GiftPaidUpgrade,
    RewardGift,
    AnonGiftPaidUpgrade,
    Raid,
    Unraid,
    Ritual,
    BitsBadgeTier
}

public enum UserNoticeRitualName
{
    ParsingError,
    NewChatter
}
