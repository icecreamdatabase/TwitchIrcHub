using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class IrcMessage
{
    public string RawSource { get; }
    public string? IrcPrefixRaw { get; }
    public IrcMessagePrefix? IrcPrefix { get; }
    public string IrcCommand { get; }
    public List<string> IrcParameters { get; }
    public Dictionary<string, string> IrcMessageTags { get; }

    public IrcMessage(string rawSource, string? ircPrefixRaw, IrcMessagePrefix? ircPrefix, string ircCommand,
        List<string> ircParameters, Dictionary<string, string> ircMessageTags)
    {
        RawSource = rawSource;
        IrcPrefixRaw = ircPrefixRaw;
        IrcPrefix = ircPrefix;
        IrcCommand = ircCommand;
        IrcParameters = ircParameters;
        IrcMessageTags = ircMessageTags;
    }
}
