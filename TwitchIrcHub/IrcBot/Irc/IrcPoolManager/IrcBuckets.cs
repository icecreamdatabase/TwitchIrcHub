using TwitchIrcHub.IrcBot.Helper;

namespace TwitchIrcHub.IrcBot.Irc.IrcPoolManager;

public class IrcBuckets
{
    private const int WaitForDelayMs = 25;
    public BasicBucket AuthenticateBucket { get; }
    public BasicBucket JoinBucket { get; }
    public IrcPrivMsgBucket IrcChannelBucket { get; }
    public IrcPrivMsgBucket IrcGlobalBucket { get; }

    public IrcBuckets(Limits limits)
    {
        AuthenticateBucket = new BasicBucket(
            limits.IrcAuthBucketLimit,
            Limits.IrcAuthBucketPerXSeconds
        );
        JoinBucket = new BasicBucket(
            limits.IrcJoinBucketLimit,
            Limits.IrcJoinBucketPerXSeconds
        );
        IrcChannelBucket = new IrcPrivMsgBucket(
            limits.IrcChannelMessageBucketLimitUser,
            limits.IrcChannelMessageBucketLimitMod,
            Limits.IrcMessageBucketPerXSeconds
        );
        IrcGlobalBucket = new IrcPrivMsgBucket(
            limits.IrcGlobalMessageBucketLimitUser,
            limits.IrcGlobalMessageBucketLimitMod,
            Limits.IrcMessageBucketPerXSeconds
        );
    }

    public async Task WaitForAuthenticateTicket(CancellationToken? cancellationToken = null)
    {
        while (!AuthenticateBucket.TakeTicket())
        {
            await Task.Delay(WaitForDelayMs, cancellationToken ?? CancellationToken.None);
        }
    }

    public async Task WaitForJoinTicket(CancellationToken? cancellationToken = null)
    {
        while (!JoinBucket.TakeTicket())
        {
            await Task.Delay(WaitForDelayMs, cancellationToken ?? CancellationToken.None);
        }
    }

    public async Task WaitForMessageTicket(bool isMod, CancellationToken? cancellationToken = null)
    {
        while (!IrcChannelBucket.TicketAvailable(isMod) || !IrcGlobalBucket.TicketAvailable(isMod))
            await Task.Delay(WaitForDelayMs, cancellationToken ?? CancellationToken.None);

        if (!IrcChannelBucket.TakeTicket(isMod))
            throw new Exception($"Taking available {nameof(IrcChannelBucket)} ticket failed!");
        if (!IrcGlobalBucket.TakeTicket(isMod))
            throw new Exception($"Taking available {nameof(IrcGlobalBucket)} ticket failed!");
    }
}
