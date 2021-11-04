using System.Collections.Concurrent;
using TwitchIrcHub.ExternalApis.Discord.WebhookObjects;

namespace TwitchIrcHub.ExternalApis.Discord;

public class DiscordLogger
{
    private const int DiscordWebhookGroupingDelay = 2000;
    private readonly ConcurrentQueue<WebhookPostContent> _messageQueue = new();

    private static DiscordLogger GetInstance { get; } = new();

    private DiscordLogger()
    {
        new Thread(ThreadRunner).Start();
    }

    public static void LogException(Exception e)
    {
        Log(
            LogLevel.Error,
            e.ToString().Split(Environment.NewLine).Select(s => s.Trim()).Take(2).ToArray()
        );
    }

    public static void Log(LogLevel logLevel, params string[] messages) => Log(logLevel, LogChannel.Main, messages);

    public static void Log(LogLevel logLevel, LogChannel logChannel, params string[] messages)
    {
        Manual(
            logLevel.ToString(),
            $"```\n{string.Join("\n``` ```\n", messages)}\n```",
            GetLogLevelColour(logLevel),
            logChannel
        );
    }

    private static void Manual(string title, string description, int color, LogChannel logChannel = LogChannel.Main)
    {
        WebhookEmbeds embed = new()
        {
            Title = title,
            Timestamp = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture),
            Description = description,
            Color = color,
            Footer = new WebhookFooter
            {
                Text = nameof(TwitchIrcHub)
            }
        };
        WebhookPostContent content = new()
        {
            Username = nameof(TwitchIrcHub),
            Embeds = new List<WebhookEmbeds> {embed},
            LogChannel = logChannel
        };
        GetInstance._messageQueue.Enqueue(content);
    }

    private static void ManualFile(string fileContent)
    {
        WebhookPostContent content = new()
        {
            Username = nameof(TwitchIrcHub),
            FileContent = fileContent,
            //PayloadJson = JsonSerializer.Serialize(
            //    new WebhookCreateMessage {Embed = new List<WebhookEmbeds> {embed}},
            //    new JsonSerializerOptions {IgnoreNullValues = true}),
        };
        GetInstance._messageQueue.Enqueue(content);
    }

    private static int GetDecimalFromHexString(string hex)
    {
        hex = hex.Replace("#", "");
        return Convert.ToInt32(hex, 16);
    }

    private static int GetLogLevelColour(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => 12648384, //#C0FFC0
            LogLevel.Debug => 8379242, //#7FDB6A
            LogLevel.Information => 15653937, //#EEDC31
            LogLevel.Warning => 14971382, //#E47200
            LogLevel.Error => 16009031, //#F44747
            LogLevel.Critical => 0, //#000000
            LogLevel.None => 16777215, //#FFFFFF
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }

    private void ThreadRunner()
    {
        while (true)
        {
            Thread.Sleep(DiscordWebhookGroupingDelay);
            if (_messageQueue.IsEmpty)
                continue;

            if (_messageQueue.TryDequeue(out WebhookPostContent? content))
            {
                if (string.IsNullOrEmpty(content.FileContent))
                {
                    DiscordWebhook.SendEmbedsWebhook(content.LogChannel, content);
                }
                else
                {
                    Dictionary<string, string> files = new()
                    {
                        {"Stacktrace", content.FileContent}
                    };

                    DiscordWebhook.SendFilesWebhook(content.LogChannel, content.Username, files,
                        content.PayloadJson);
                }
            }
        }
    }
}