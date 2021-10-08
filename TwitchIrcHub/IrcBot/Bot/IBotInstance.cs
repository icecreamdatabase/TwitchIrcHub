namespace TwitchIrcHub.IrcBot.Bot;

public interface IBotInstance : IDisposable
{
    void Init(int botUserId);

    void IntervalPing();
    
    IBotInstanceData BotInstanceData { get; }
}