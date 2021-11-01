using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes.Parsed;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class IrcNotice
{
    public IrcMessage Raw { get; }

    /* --------------------------------------------------------------------------- */
    /* ------------------------------ Required tags ------------------------------ */
    /* --------------------------------------------------------------------------- */
    public NoticeMessageId MessageId { get; }

    /* --------------------------------------------------------------------------- */
    /* --------------------- Non-tag but still required data --------------------- */
    /* --------------------------------------------------------------------------- */
    public string RoomName { get; }
    public string Message { get; }


    public IrcNotice(IrcMessage ircMessage)
    {
        if (ircMessage.IrcCommand != IrcCommands.Notice)
            throw new ArgumentOutOfRangeException(nameof(ircMessage), "Input is not a Notice");

        Raw = ircMessage;

        // Try parsing all known tags.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        ircMessage.IrcMessageTags.TryGetValue("msg-id", out string? messageId);

        if (ircMessage.IrcParameters.Count < 2)
            throw new Exception($"NOTICE without valid roomName or message:\n{ircMessage.RawSource}");

        // Assign parsed tags to properties.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        MessageId = Enum.TryParse(
            messageId?.Replace("_", ""),
            true,
            out NoticeMessageId parsedMessageId
        )
            ? parsedMessageId
            : NoticeMessageId.ParsingError;

        /* --------------------------------------------------------------------------- */
        /* --------------------- Non-tag but still required data --------------------- */
        /* --------------------------------------------------------------------------- */
        Message = ircMessage.IrcParameters[1];
        RoomName = ircMessage.IrcParameters[0][1..];
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum NoticeMessageId
{
    ParsingError,
    AlreadyBanned,
    AlreadyEmoteOnlyOff,
    AlreadyEmoteOnlyOn,
    AlreadyR9kOff,
    AlreadyR9kOn,
    AlreadySubsOff,
    AlreadySubsOn,
    BadBanAdmin,
    BadBanAnon,
    BadBanBroadcaster,
    BadBanGlobalMod,
    BadBanMod,
    BadBanSelf,
    BadBanStaff,
    BadCommercialError,
    BadDeleteMessageBroadcaster,
    BadDeleteMessageMod,
    BadHostError,
    BadHostHosting,
    BadHostRateExceeded,
    BadHostRejected,
    BadHostSelf,
    BadMarkerClient,
    BadModBanned,
    BadModMod,
    BadSlowDuration,
    BadTimeoutAdmin,
    BadTimeoutAnon,
    BadTimeoutBroadcaster,
    BadTimeoutDuration,
    BadTimeoutGlobalMod,
    BadTimeoutMod,
    BadTimeoutSelf,
    BadTimeoutStaff,
    BadUnbanNoBan,
    BadUnhostError,
    BadUnmodMod,
    BanSuccess,
    CmdsAvailable,
    ColorChanged,
    CommercialSuccess,
    DeleteMessageSuccess,
    EmoteOnlyOff,
    EmoteOnlyOn,
    FollowersOff,
    FollowersOn,
    FollowersOnzero,
    HostOff,
    HostOn,
    HostSuccess,
    HostSuccessViewers,
    HostTargetWentOffline,
    HostsRemaining,
    InvalidUser,
    ModSuccess,
    MsgBanned,
    MsgBadCharacters,
    MsgChannelBlocked,
    MsgChannelSuspended,
    MsgDuplicate,
    MsgEmoteonly,
    MsgFacebook,
    MsgFollowersonly,
    MsgFollowersonlyFollowed,
    MsgFollowersonlyZero,
    MsgR9k,
    MsgRatelimit,
    MsgRejected,
    MsgRejectedMandatory,
    MsgRoomNotFound,
    MsgSlowmode,
    MsgSubsonly,
    MsgSuspended,
    MsgTimedout,
    MsgVerifiedEmail,
    NoHelp,
    NoMods,
    NotHosting,
    NoPermission,
    R9kOff,
    R9kOn,
    RaidErrorAlreadyRaiding,
    RaidErrorForbidden,
    RaidErrorSelf,
    RaidErrorTooManyViewers,
    RaidErrorUnexpected,
    RaidNoticeMature,
    RaidNoticeRestrictedChat,
    RoomMods,
    SlowOff,
    SlowOn,
    SubsOff,
    SubsOn,
    TimeoutNoTimeout,
    TimeoutSuccess,
    TosBan,
    TurboOnlyColor,
    UnbanSuccess,
    UnmodSuccess,
    UnraidErrorNoActiveRaid,
    UnraidErrorUnexpected,
    UnraidSuccess,
    UnrecognizedCmd,
    UnsupportedChatroomsCmd,
    UntimeoutBanned,
    UntimeoutSuccess,
    UsageBan,
    UsageClear,
    Clear,
    UsageColor,
    UsageCommercial,
    UsageDisconnect,
    UsageEmoteOnlyOff,
    UsageEmoteOnlyOn,
    UsageFollowersOff,
    UsageFollowersOn,
    UsageHelp,
    UsageHost,
    UsageMarker,
    UsageMe,
    UsageMod,
    UsageMods,
    UsageR9kOff,
    UsageR9kOn,
    UsageRaid,
    UsageSlowOff,
    UsageSlowOn,
    UsageSubsOff,
    UsageSubsOn,
    UsageTimeout,
    UsageUnban,
    UsageUnhost,
    UsageUnmod,
    UsageUnraid,
    UsageUntimeout,
    WhisperBanned,
    WhisperBannedRecipient,
    WhisperInvalidArgs,
    WhisperInvalidLogin,
    WhisperInvalidSelf,
    WhisperLimitPerMin,
    WhisperLimitPerSec,
    WhisperRestricted,
    WhisperRestrictedRecipient
}
