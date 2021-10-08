namespace TwitchIrcHub.IrcBot.Bot;

public interface IBotInstanceData
{
    public int UserId { get; }
    public string UserName { get; }
    public string AccessToken { get; }
    public string RefreshToken { get; }
    public int? SupinicApiUser { get; }
    public string? SupinicApiKey { get; }

    public void Init(int botUserId);
    public void IntervalPing();
}
