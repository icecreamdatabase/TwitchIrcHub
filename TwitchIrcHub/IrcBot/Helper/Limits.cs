namespace TwitchIrcHub.IrcBot.Helper;

public class Limits
{
    public static readonly Limits NormalBot = new()
    {
        SendConnections = 2,
        MaxChannelsPerIrcClient = 50,
        IrcAuthBucketLimit = 20,
        IrcJoinBucketLimit = 20,
        IrcMessageBucketLimitUser = 20,
        IrcMessageBucketLimitMod = 100,
    };


    public static readonly Limits KnownBot = new()
    {
        SendConnections = 2,
        MaxChannelsPerIrcClient = 50,
        IrcAuthBucketLimit = 20,
        IrcJoinBucketLimit = 20,
        IrcMessageBucketLimitUser = 50,
        IrcMessageBucketLimitMod = 100,
    };

    public static readonly Limits VerifiedBot = new()
    {
        SendConnections = 5,
        MaxChannelsPerIrcClient = 500,
        IrcAuthBucketLimit = 200,
        IrcJoinBucketLimit = 2000,
        IrcMessageBucketLimitUser = 7500,
        IrcMessageBucketLimitMod = 7500,
    };

    public int SendConnections { get; private init; }
    public int MaxChannelsPerIrcClient { get; private init; }
        
    /* IRC connect bucket */
    public int IrcAuthBucketLimit { get; private init; }
    public int IrcAuthBucketPerXSeconds { get; private init; } = 10;
    public int IrcJoinBucketLimit { get; private init; }
    public int IrcJoinBucketPerXSeconds { get; private init; } = 10;
        
    /* IRC message bucket */
    public int IrcMessageBucketLimitUser { get; private init; }
    public int IrcMessageBucketLimitMod { get; private init; }
    public int IrcMessageBucketPerXSeconds { get; private init; } = 30;
}