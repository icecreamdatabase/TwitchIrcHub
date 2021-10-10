namespace TwitchIrcHub.IrcBot.Irc.DataTypes;

public class IrcMessage
{
    public string RawSource { get; init; }
    public string IrcPrefixRaw { get; init; }
    public IrcMessagePrefix IrcPrefix { get; init; }
    public string IrcCommand { get; init; }
    public List<string> IrcParameters { get; init; }
    public Dictionary<string, string> IrcMessageTags { get; init; }
}