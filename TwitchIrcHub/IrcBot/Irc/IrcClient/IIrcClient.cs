using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.IrcPoolManager;

namespace TwitchIrcHub.IrcBot.Irc.IrcClient;

public interface IIrcClient
{
    public void Init(IIrcPoolManager ircPoolManager);
    public BulkObservableCollection<string> Channels { get; }
    public Task Shutdown();
}