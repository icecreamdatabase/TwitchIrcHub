namespace TwitchIrcHub.IrcBot.Irc.DataTypes;

public static class IrcParseHelper
{
    public static Dictionary<string, string> ParseBadgeData(string? value)
    {
        if (value == null)
            return new Dictionary<string, string>();

        return value.Split(',')
            .Where(input => input.Contains('/'))
            .Select(input => input.Split('/'))
            .ToDictionary(
                split => split[0],
                split => split[1]
            );
    }

    public static Dictionary<string, string[]> ParseEmoteData(string? value)
    {
        if (value == null)
            return new Dictionary<string, string[]>();

        return value.Split('/')
            .Where(input => input.Contains(':'))
            .Select(input => input.Split(':'))
            .ToDictionary(
                split => split[0],
                split => split[1].Split(',')
            );
    }

    public static Dictionary<string, string[]> ParseFlags(string? value)
    {
        if (value == null)
            return new Dictionary<string, string[]>();

        return value.Split(',')
            .Where(input => input.Contains(':'))
            .Select(input => input.Split(':'))
            .ToDictionary(
                split => split[0],
                split => split[1].Split('/')
            );
    }
}
