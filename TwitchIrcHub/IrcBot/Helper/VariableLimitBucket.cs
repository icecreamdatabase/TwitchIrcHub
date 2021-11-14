namespace TwitchIrcHub.IrcBot.Helper;

public class VariableLimitBucket 
{
    private readonly int _perXSeconds;
    private int _usedTickets;
    private protected const float LimitBuffer = 0.8f;
    private const float TimeBuffer = 1.1f;
    
    public VariableLimitBucket(int perXSeconds)
    {
        _perXSeconds = perXSeconds;
    }
    
    public int TicketsRemaining(int currentTicketLimit) => currentTicketLimit - _usedTickets;

    public bool TicketAvailable(int currentTicketLimit) => TicketsRemaining(currentTicketLimit) > 0;

    public bool TakeTicket(int currentTicketLimit, int amountToTake = 1)
    {
        if (_usedTickets + amountToTake > currentTicketLimit)
            return false;

        _usedTickets += amountToTake;
        Task.Delay(new TimeSpan(0, 0, _perXSeconds)).ContinueWith(ReturnTicket);
        return true;
    }

    private void ReturnTicket(Task task)
    {
        if (_usedTickets > 0)
            _usedTickets--;
    }
}
