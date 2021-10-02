namespace TwitchIrcHub.IrcBot.Bot;

public interface IBotInstance : IDisposable
{
    void Init(int botUserId);

    void Update();
    
    int BotUserId { get; }
}