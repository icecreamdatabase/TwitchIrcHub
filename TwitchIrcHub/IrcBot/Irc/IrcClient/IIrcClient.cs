using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.IrcPoolManager;

namespace TwitchIrcHub.IrcBot.Irc.IrcClient;

public interface IIrcClient
{
    public void Init(IIrcPoolManager ircPoolManager, bool isSendOnlyConnection = false);
    public BulkObservableCollection<string> Channels { get; }
    public Task Shutdown();
    public Task SendLine(string line);
}
