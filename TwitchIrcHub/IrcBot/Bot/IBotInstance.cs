namespace TwitchIrcHub.IrcBot.Bot;

public interface IBotInstance : IDisposable
{
    public Task Init(int botUserId);

    public Task IntervalPing();
    
    public IBotInstanceData BotInstanceData { get; }
}