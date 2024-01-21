using DifBot.Config;
using DifBot.Workers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DifBot.Services.Loggers;

public class DiscordLogger
{
    private readonly BotOptions _options;
    private readonly DiscordLogChannel _channel;
    private readonly ILogger<DiscordLogger> _logger;

    public DiscordLogger(IOptions<BotOptions> options, DiscordLogChannel channel, ILogger<DiscordLogger> logger)
    {
        _options = options.Value;
        _channel = channel;
        _logger = logger;
    }

    public void LogMessage(string message, ulong channelId)
    {
        ulong guildId = _options.GuildId;

        var discordLogItem = new DiscordLogItem<string>(message, guildId, channelId);

        var sucess = _channel.Writer.TryWrite(discordLogItem);

        if (!sucess)
        {
            _logger.LogError("Could not write to DiscordLogChannel");
        }
    }
}
