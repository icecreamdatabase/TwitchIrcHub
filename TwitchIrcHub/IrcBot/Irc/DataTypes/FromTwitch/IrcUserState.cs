using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class IrcUserState
{
    public IrcMessage Raw { get; }

    /* --------------------------------------------------------------------------- */
    /* ------------------------------ Required tags ------------------------------ */
    /* --------------------------------------------------------------------------- */
    public Dictionary<string, int> BadgeInfo { get; }
    public Dictionary<string, int> Badges { get; }
    public string Color { get; }
    public string DisplayName { get; }
    public string[] EmoteSets { get; }
    public bool IsMod { get; }
    public bool IsSubscriber { get; }
    //TODO: UserType enum
    public string? UserType { get; }

    /* --------------------------------------------------------------------------- */
    /* --------------------- Non-tag but still required data --------------------- */
    /* --------------------------------------------------------------------------- */
    public string RoomName { get; }


    public IrcUserState(IrcMessage ircMessage)
    {
        if (ircMessage.IrcCommand != IrcCommands.UserState)
            throw new ArgumentOutOfRangeException(nameof(ircMessage), "Input is not a UserState");

        Raw = ircMessage;

        // Try parsing all known tags.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        ircMessage.IrcMessageTags.TryGetValue("badge-info", out string? badgeInfo);
        ircMessage.IrcMessageTags.TryGetValue("badges", out string? badges);
        ircMessage.IrcMessageTags.TryGetValue("color", out string? color);
        ircMessage.IrcMessageTags.TryGetValue("display-name", out string? displayName);
        ircMessage.IrcMessageTags.TryGetValue("emote-sets", out string? emoteSets);
        ircMessage.IrcMessageTags.TryGetValue("mod", out string? mod);
        ircMessage.IrcMessageTags.TryGetValue("subscriber", out string? subscriber);
        ircMessage.IrcMessageTags.TryGetValue("user-type", out string? userType);

        // Exceptions for nullable / missing tags that are not allowed to be missing.
        if (string.IsNullOrEmpty(displayName))
            throw new Exception($"USERSTATE without valid displayName:\n{ircMessage.RawSource}");
        if (ircMessage.IrcParameters.Count < 1)
            throw new Exception($"USERSTATE without valid roomName:\n{ircMessage.RawSource}");

        // Assign parsed tags to properties.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        BadgeInfo = IrcParseHelper.ParseBadgeData(badgeInfo);
        Badges = IrcParseHelper.ParseBadgeData(badges);
        Color = color ?? "";
        DisplayName = displayName;
        EmoteSets = emoteSets?.Split(',') ?? Array.Empty<string>();
        IsMod = mod != null && mod != "0";
        IsSubscriber = subscriber != null && subscriber != "0";
        UserType = userType;

        /* --------------------------------------------------------------------------- */
        /* --------------------- Non-tag but still required data --------------------- */
        /* --------------------------------------------------------------------------- */
        RoomName = ircMessage.IrcParameters[0][1..];
    }
}
