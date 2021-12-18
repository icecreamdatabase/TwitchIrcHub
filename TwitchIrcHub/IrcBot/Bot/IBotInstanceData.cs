using TwitchIrcHub.IrcBot.Helper;

namespace TwitchIrcHub.IrcBot.Bot;

public interface IBotInstanceData
{
    public int UserId { get; }
    public string UserName { get; }
    public bool EnabledWhisperLog { get; }
    public bool Known { get; }
    public bool Verified { get; }
    public string AccessToken { get; }
    public string RefreshToken { get; }
    public int? SupinicApiUser { get; }
    public string? SupinicApiKey { get; }
    public Limits Limits { get; }
    public Task Init(int botUserId);
    public Task IntervalPing();
    public Task ValidateAccessToken();
}
