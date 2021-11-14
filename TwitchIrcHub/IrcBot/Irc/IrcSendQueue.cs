using TwitchIrcHub.IrcBot.Irc.DataTypes.ToTwitch;

namespace TwitchIrcHub.IrcBot.Irc;

public class IrcSendQueue
{
    private readonly IrcPoolManager.IrcPoolManager _ircPoolManager;

    private Task _previousTask = Task.FromResult(true);
    private readonly object _key = new();

    public IrcSendQueue(IrcPoolManager.IrcPoolManager ircPoolManager)
    {
        _ircPoolManager = ircPoolManager;
    }

    public Task Enqueue(PrivMsgToTwitch privMsgToTwitch)
    {
        lock (_key)
        {
            _previousTask = _previousTask.ContinueWith(
                _ => _ircPoolManager.SendMessageNoQueue(privMsgToTwitch),
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default
            );
            return _previousTask;
        }
    }
}
