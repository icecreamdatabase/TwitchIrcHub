namespace TwitchIrcHub.IrcBot.Helper;

public class BasicBucket
{
    private readonly int _limit;
    private readonly int _perXSeconds;
    private int _usedTickets;
    private const float LimitBuffer = 0.8f;
    private const float TimeBuffer = 1.1f;

    public BasicBucket(int limit, int perXSeconds)
    {
        _limit = (int)(limit * LimitBuffer);
        _perXSeconds = (int)(perXSeconds * TimeBuffer);
    }

    public int TicketsRemaining => _limit - _usedTickets;

    public bool TicketAvailable => TicketsRemaining > 0;

    public bool TakeTicket(int amount = 1)
    {
        if (_usedTickets + amount > _limit)
            return false;

        _usedTickets += amount;
        Task.Delay(new TimeSpan(0, 0, _perXSeconds)).ContinueWith(ReturnTicket);
        return true;
    }

    private void ReturnTicket(Task task)
    {
        if (_usedTickets > 0)
            _usedTickets--;
    }
}
