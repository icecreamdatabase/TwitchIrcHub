using System.Collections.Concurrent;
using TwitchIrcHub.IrcBot.Irc.DataTypes.ToTwitch;

namespace TwitchIrcHub.IrcBot.Irc;

public class IrcSendQueue
{
    private readonly IrcPoolManager.IrcPoolManager _ircPoolManager;
    private readonly ConcurrentQueue<PrivMsgToTwitch> _queue = new();

    private Task _currentCheckQueueTask = Task.CompletedTask;

    public IrcSendQueue(IrcPoolManager.IrcPoolManager ircPoolManager)
    {
        _ircPoolManager = ircPoolManager;
    }

    public void Enqueue(PrivMsgToTwitch privMsgToTwitch)
    {
        _queue.Enqueue(privMsgToTwitch);
        if (_currentCheckQueueTask.IsCompleted)
        {
            _currentCheckQueueTask = Task.Run(CheckQueue);
        }
    }

    private async Task CheckQueue()
    {
        while (_queue.TryDequeue(out PrivMsgToTwitch? privMsgToTwitch))
            await _ircPoolManager.SendMessageNoQueue(privMsgToTwitch);
    }
}
