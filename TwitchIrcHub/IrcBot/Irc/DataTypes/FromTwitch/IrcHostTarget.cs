using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class IrcHostTarget
{
    public IrcMessage Raw { get; }

    /* --------------------------------------------------------------------------- */
    /* --------------------- Non-tag but still required data --------------------- */
    /* --------------------------------------------------------------------------- */
    public string? HostReceiverRoomName { get; }
    public string RoomName { get; }
    public int? NumberOfViews { get; }

    public IrcHostTarget(IrcMessage ircMessage)
    {
        if (ircMessage.IrcCommand != IrcCommands.HostTarget)
            throw new ArgumentOutOfRangeException(nameof(ircMessage), "Input is not a HostTarget");

        Raw = ircMessage;

        if (ircMessage.IrcParameters.Count < 2)
            throw new Exception($"HOSTTARGET without valid roomName or target:\n{ircMessage.RawSource}");

        /* --------------------------------------------------------------------------- */
        /* --------------------- Non-tag but still required data --------------------- */
        /* --------------------------------------------------------------------------- */
        string[] trailingSplit = ircMessage.IrcParameters[1].Split(" ", 2);
        if (trailingSplit.Length >= 1 && trailingSplit[0] != "-")
            HostReceiverRoomName = trailingSplit[0];
        if (trailingSplit.Length >= 2)
            NumberOfViews = int.Parse(trailingSplit[1]);
        RoomName = ircMessage.IrcParameters[0][1..];
    }
}
