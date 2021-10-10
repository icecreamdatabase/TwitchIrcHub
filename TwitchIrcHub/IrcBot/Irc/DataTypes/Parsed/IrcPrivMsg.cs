using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes.Parsed;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class IrcPrivMsg
{
    public IrcMessage Raw { get; }
    public Dictionary<string, int>? BadgeInfo { get; }
    public Dictionary<string, int>? Badges { get; }
    public string? Bits { get; }
    public string? Color { get; }
    public string? CustomRewardId { get; }
    public string? DisplayName { get; }
    public string[] Emotes { get; }
    public bool FirstMsg { get; }
    public string? Flags { get; }
    public string Id { get; }
    public int RoomId { get; }
    public string RoomName { get; }
    public DateTime TmiSentTs { get; }
    public int UserId { get; }
    public string UserName { get; }

    public string? Message { get; }

    public IrcPrivMsg(IrcMessage ircMessage)
    {
        if (ircMessage.IrcCommand != IrcCommands.PrivMsg)
            throw new ArgumentOutOfRangeException(nameof(ircMessage), "Input is not a PrivMsg");

        Raw = ircMessage;

        // Try parsing all known tags.
        ircMessage.IrcMessageTags.TryGetValue("badge-info", out string? badgeInfo);
        ircMessage.IrcMessageTags.TryGetValue("badges", out string? badges);
        ircMessage.IrcMessageTags.TryGetValue("bits", out string? bits);
        ircMessage.IrcMessageTags.TryGetValue("color", out string? color);
        ircMessage.IrcMessageTags.TryGetValue("custom-reward-id", out string? customRewardId);
        ircMessage.IrcMessageTags.TryGetValue("display-name", out string? displayName);
        ircMessage.IrcMessageTags.TryGetValue("emotes", out string? emotes);
        ircMessage.IrcMessageTags.TryGetValue("first-msg", out string? firstMsg);
        ircMessage.IrcMessageTags.TryGetValue("flags", out string? flags);
        ircMessage.IrcMessageTags.TryGetValue("id", out string? id);
        //ircMessage.IrcMessageTags.TryGetValue("mod", out string? mod);
        ircMessage.IrcMessageTags.TryGetValue("room-id", out string? roomId);
        //ircMessage.IrcMessageTags.TryGetValue("subscriber", out string? subscriber);
        ircMessage.IrcMessageTags.TryGetValue("tmi-sent-ts", out string? tmiSentTs);
        //ircMessage.IrcMessageTags.TryGetValue("turbo", out string? turbo);
        ircMessage.IrcMessageTags.TryGetValue("user-id", out string? userId);
        //ircMessage.IrcMessageTags.TryGetValue("user-type", out string? userType);

        // Exceptions for missing tags that are not allowed to be missing.
        if (string.IsNullOrEmpty(id))
            throw new Exception($"PrivMsg without id:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(roomId))
            throw new Exception($"PrivMsg without valid roomId:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(userId))
            throw new Exception($"PrivMsg without valid userId:\n{ircMessage.RawSource}");
        if (ircMessage.IrcParameters.Count <= 1)
            throw new Exception($"PrivMsg without roomName or message:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(ircMessage.IrcPrefix.Username))
            throw new Exception($"PrivMsg without userName:\n{ircMessage.RawSource}");
        if (string.IsNullOrEmpty(tmiSentTs))
            throw new Exception($"PrivMsg without timestamp:\n{ircMessage.RawSource}");

        // Assign parsed tags to properties.
        BadgeInfo = ParseIrcDictionary(badgeInfo);
        Badges = ParseIrcDictionary(badges);
        Bits = bits;
        Color = color;
        CustomRewardId = customRewardId;
        DisplayName = displayName;
        Emotes = emotes?.Split('/') ?? Array.Empty<string>();
        FirstMsg = firstMsg == "1";
        Flags = flags;
        Id = id;
        RoomId = int.Parse(roomId);
        TmiSentTs = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(tmiSentTs)).UtcDateTime;
        UserId = int.Parse(userId);

        RoomName = ircMessage.IrcParameters[0][1..];
        Message = ircMessage.IrcParameters[1];
        UserName = ircMessage.IrcPrefix.Username;
    }

    private static Dictionary<string, int>? ParseIrcDictionary(string? value)
    {
        return value?.Split(',')
            .Select(input => input.Split('/'))
            .ToDictionary(
                split => split[0],
                split => int.Parse(split[1])
            );
    }
}
