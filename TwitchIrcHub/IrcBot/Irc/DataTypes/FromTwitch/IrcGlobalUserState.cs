﻿using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class IrcGlobalUserState
{
    public IrcMessage Raw { get; }

    /* --------------------------------------------------------------------------- */
    /* ------------------------------ Required tags ------------------------------ */
    /* --------------------------------------------------------------------------- */
    public Dictionary<string, string> BadgeInfo { get; }
    public Dictionary<string, string> Badges { get; }
    public string Color { get; }
    public string DisplayName { get; }
    public string[] EmoteSets { get; }
    public int UserId { get; }
    //TODO: UserType enum
    public string? UserType { get; }


    public IrcGlobalUserState(IrcMessage ircMessage)
    {
        if (ircMessage.IrcCommand != IrcCommands.GlobalUserState)
            throw new ArgumentOutOfRangeException(nameof(ircMessage), "Input is not a GlobalUserState");

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
        ircMessage.IrcMessageTags.TryGetValue("user-id", out string? userId);
        ircMessage.IrcMessageTags.TryGetValue("user-type", out string? userType);

        // Exceptions for nullable / missing tags that are not allowed to be missing.
        if (string.IsNullOrEmpty(displayName))
            throw new Exception($"PRIVMSG without valid displayName:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(userId))
            throw new Exception($"PRIVMSG without valid userId:\n{ircMessage.RawSource}");

        // Assign parsed tags to properties.
        /* --------------------------------------------------------------------------- */
        /* ------------------------------ Required tags ------------------------------ */
        /* --------------------------------------------------------------------------- */
        BadgeInfo = IrcParseHelper.ParseBadgeData(badgeInfo);
        Badges = IrcParseHelper.ParseBadgeData(badges);
        Color = color ?? "";
        DisplayName = displayName;
        EmoteSets = emoteSets?.Split(',') ?? Array.Empty<string>();
        UserId = int.Parse(userId);
        UserType = userType;
    }
}
