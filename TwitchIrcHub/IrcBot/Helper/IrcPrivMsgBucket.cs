namespace TwitchIrcHub.IrcBot.Helper;

public class IrcPrivMsgBucket : VariableLimitBucket
{
    private readonly int _limitUser;
    private readonly int _limitMod;

    public IrcPrivMsgBucket(int limitUser, int limitMod, int perXSeconds) : base(perXSeconds)
    {
        _limitUser = (int)(limitUser * LimitBuffer);
        _limitMod = (int)(limitMod * LimitBuffer);
    }

    public int TicketsRemaining(bool isUserModInTargetChannel)
    {
        return TicketsRemaining(GetTicketLimitFromIrcDat(isUserModInTargetChannel));
    }

    public bool TicketAvailable(bool isUserModInTargetChannel)
    {
        return TicketAvailable(GetTicketLimitFromIrcDat(isUserModInTargetChannel));
    }

    public bool TakeTicket(bool isUserModInTargetChannel)
    {
        return base.TakeTicket(GetTicketLimitFromIrcDat(isUserModInTargetChannel));
    }

    private int GetTicketLimitFromIrcDat(bool isUserModInTargetChannel)
    {
        return isUserModInTargetChannel ? _limitMod : _limitUser;
    }
}
