using System.Diagnostics.CodeAnalysis;

namespace TwitchIrcHub.IrcBot.Irc.DataTypes;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class IrcCommands
{
    public const string RplWelcome = "001";
    public const string RplYourHost = "002";
    public const string RplCreated = "003";
    public const string RplMyInfo = "004";
    public const string RplNamReply = "353";
    public const string RplEndOfNames = "366";
    public const string RplMotd = "372";
    public const string RplMotdStart = "375";
    public const string RplEndOfMotd = "376";
    public const string Cap = "CAP";
    public const string ClearChat = "CLEARCHAT";
    public const string ClearMsg = "CLEARMSG";
    public const string GlobalUserState = "GLOBALUSERSTATE";
    public const string HostTarget = "HOSTTARGET";
    public const string Join = "JOIN";
    public const string Notice = "NOTICE";
    public const string Part = "PART";
    public const string Ping = "PING";
    public const string Pong = "PONG";
    public const string PrivMsg = "PRIVMSG";
    public const string Reconnect = "RECONNECT";
    public const string RoomState = "ROOMSTATE";
    public const string UserNotice = "USERNOTICE";
    public const string UserState = "USERSTATE";
    public const string Whisper = "WHISPER";
}