using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes.Parsed;

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

        /* --------------------------------------------------------------------------- */
        /* --------------------- Non-tag but still required data --------------------- */
        /* --------------------------------------------------------------------------- */
        RoomName = ircMessage.IrcParameters[0][1..];
    }
}
