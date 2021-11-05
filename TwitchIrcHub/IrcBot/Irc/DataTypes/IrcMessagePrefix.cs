using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class IrcMessagePrefix
{
    public string? Nickname { get; }
    public string? Username { get; }
    public string? Hostname { get; }

    public IrcMessagePrefix(string? nickname, string? username, string? hostname)
    {
        Nickname = nickname;
        Username = username;
        Hostname = hostname;
    }
}
