namespace TwitchIrcHub.IrcBot.Irc.DataTypes;

public class IrcMessagePrefix
{
    public string? Nickname { get; init; }
    public string? Username { get; init; }
    public string Hostname { get; init; }
}