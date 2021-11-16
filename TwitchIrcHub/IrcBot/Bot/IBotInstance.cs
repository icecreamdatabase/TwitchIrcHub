using TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;
using TwitchIrcHub.IrcBot.Irc.DataTypes.ToTwitch;

namespace TwitchIrcHub.IrcBot.Bot;

public interface IBotInstance : IDisposable
{
    public Task Init(int botUserId);
    public Task IntervalPing();
    public IBotInstanceData BotInstanceData { get; }
    public void SendPrivMsg(PrivMsgToTwitch privMsg);
    public Task<List<IrcUserState>> GetUserStatesForChannels(List<int> roomIds);
    public IrcGlobalUserState? GetGlobalUserState();
}
