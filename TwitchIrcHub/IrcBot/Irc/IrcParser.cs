using System.Text.RegularExpressions;
using TwitchIrcHub.IrcBot.Irc.DataTypes;

namespace TwitchIrcHub.IrcBot.Irc;

public static class IrcParser
{
    private static readonly Regex ValidCmdRegex = new(@"^(?:[a-zA-Z]+|[0-9]{3})$");

    public static IrcMessage Parse(string line)
    {
        string remainder = line;

        Dictionary<string, string> ircTags = new();
        if (line.StartsWith("@"))
        {
            remainder = remainder[1..]; // remove @ sign

            int spaceIdx = remainder.IndexOf(" ", StringComparison.Ordinal);
            if (spaceIdx < 0)
                return null;

            string tagsSrc = remainder[..spaceIdx];

            if (tagsSrc.Length == 0)
                return null;

            ircTags = ParseTags(tagsSrc);
            remainder = remainder[(spaceIdx + 1)..];
        }

        IrcMessagePrefix ircPrefix;
        string ircPrefixRaw;
        if (remainder.StartsWith(":"))
        {
            remainder = remainder[1..]; // remove : sign

            int spaceIdx = remainder.IndexOf(" ", StringComparison.Ordinal);
            if (spaceIdx < 0)
                return null;

            ircPrefixRaw = remainder[..spaceIdx];
            remainder = remainder[(spaceIdx + 1)..];

            if (ircPrefixRaw.Length == 0)
                return null;

            if (!ircPrefixRaw.Contains("@"))
            {
                // just a hostname or just a nickname
                ircPrefix = new IrcMessagePrefix
                {
                    Nickname = null,
                    Username = null,
                    Hostname = ircPrefixRaw
                };
            }
            else
            {
                // full prefix (nick[[!user]@host])
                // valid forms:
                // nick (but this is not really possible to differentiate
                //       from the hostname only, so if we don't get any @
                //       we just assume it's a hostname.)
                // nick@host
                // nick!user@host

                // split on @ first, then on !
                int atIndex = ircPrefixRaw.IndexOf("@", StringComparison.Ordinal);
                string nickAndUser = ircPrefixRaw[..atIndex];
                string host = ircPrefixRaw[(atIndex + 1)..];

                // now nickAndUser is either "nick" or "nick!user"
                // => split on !
                int exclamationIndex = nickAndUser.IndexOf("!", StringComparison.Ordinal);
                string nick;
                string user;
                if (exclamationIndex < 0)
                {
                    // no ! found
                    nick = nickAndUser;
                    user = null;
                }
                else
                {
                    nick = nickAndUser[..exclamationIndex];
                    user = nickAndUser[(exclamationIndex + 1)..];
                }

                if (host.Length == 0 || nick.Length == 0 || user is {Length: 0})
                    return null;

                ircPrefix = new IrcMessagePrefix
                {
                    Nickname = nick,
                    Username = user,
                    Hostname = host
                };
            }
        }
        else
        {
            ircPrefix = null;
            ircPrefixRaw = null;
        }

        int spaceAfterCommandIdx = remainder.IndexOf(" ", StringComparison.Ordinal);

        string ircCommand;
        List<string> ircParameters = new();

        if (spaceAfterCommandIdx < 0)
        {
            // no space after commands, i.e. no params.
            ircCommand = remainder;
            ;
        }
        else
        {
            // split command off
            ircCommand = remainder[..spaceAfterCommandIdx];
            remainder = remainder[(spaceAfterCommandIdx + 1)..];

            // introduce a new variable so it can be null (typescript shenanigans)
            string paramsRemainder = remainder;
            while (paramsRemainder != null)
            {
                if (paramsRemainder.StartsWith(":"))
                {
                    // trailing param, remove : and consume the rest of the input
                    ircParameters.Add(paramsRemainder[1..]);
                    paramsRemainder = null;
                }
                else
                {
                    // middle param
                    int spaceIdx = paramsRemainder.IndexOf(" ", StringComparison.Ordinal);

                    string param;
                    if (spaceIdx < 0)
                    {
                        // no space found
                        param = paramsRemainder;
                        paramsRemainder = null;
                    }
                    else
                    {
                        param = paramsRemainder[..spaceIdx];
                        paramsRemainder = paramsRemainder[(spaceIdx + 1)..];
                    }

                    if (param.Length == 0)
                        return null;

                    ircParameters.Add(param);
                }
            }
        }

        if (!ValidCmdRegex.IsMatch(ircCommand))
            return null;

        ircCommand = ircCommand.ToUpperInvariant();

        return new IrcMessage
        {
            RawSource = line,
            IrcPrefixRaw = ircPrefixRaw,
            IrcPrefix = ircPrefix,
            IrcCommand = ircCommand,
            IrcParameters = ircParameters,
            IrcMessageTags = ircTags,
        };
    }

    private static Dictionary<string, string> ParseTags(string tagsSrc)
    {
        if (tagsSrc == null)
        {
            return new Dictionary<string, string>();
        }

        return tagsSrc
            .Split(';')
            .Select(tagSrc => tagSrc.Split('=', 2))
            .ToDictionary(tag => tag[0].ToLowerInvariant(), tag => Decode(tag[1]));
    }

    private static string Decode(string input)
    {
        return input
            .Replace("\\", "\\")
            .Replace("\\:", ";")
            .Replace("\\s", " ")
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\", "");
    }
}