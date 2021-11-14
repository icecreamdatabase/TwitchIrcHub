using System.Text;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes.ToTwitch;

public class PrivMsgToTwitch
{
    public int BotUserId { get; }
    public string RoomName { get; }
    public string Message { get; }
    public string? ClientNonce { get; }
    public string? ReplyParentMsgId { get; }
    public bool UseSameSendConnectionAsPreviousMsg { get; set; }

    public PrivMsgToTwitch(int botUserId, string roomName, string message, string? clientNonce = null, string? replyParentMsgId = null,
        bool useSameSendConnectionAsPreviousMsg = false)
    {
        BotUserId = botUserId;
        RoomName = roomName.StartsWith('#') ? roomName : $"#{roomName}";
        Message = message;
        ClientNonce = clientNonce;
        ReplyParentMsgId = replyParentMsgId;
        UseSameSendConnectionAsPreviousMsg = useSameSendConnectionAsPreviousMsg;
    }

    public override string ToString()
    {
        //@client-nonce=xxx;reply-parent-msg-id=xxx PRIVMSG #channel :xxxxxx `
        StringBuilder sb = new StringBuilder();
        
        Dictionary<string, string> preamble = new();
        if (ClientNonce != null)
            preamble.Add("client-nonce", ClientNonce);
        if (ReplyParentMsgId != null)
            preamble.Add("reply-parent-msg-id", ReplyParentMsgId);

        if (preamble is { Count: > 0 })
        {
            sb.Append('@');
            sb.AppendJoin(";", preamble.Select(pair => $"{pair.Key}={pair.Value}"));
            sb.Append(' ');
        }

        sb.Append("PRIVMSG ");
        if (!RoomName.StartsWith('#'))
            sb.Append('#');
        sb.Append(RoomName);
        sb.Append(" :");
        sb.Append(Message);

        return sb.ToString();
    }
}
