﻿using System;
using System.Threading.Tasks;
using DifBot.Config;
using DifBot.Services.Loggers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Options;

namespace DifBot.Attributes;

/// <summary>
/// Reject commands coming from DM or from other guild (if it exists).
/// </summary>
public class BaseCommandCheck : CheckBaseAttribute
{
    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        object? botOptionsObj = ctx.Services.GetService(typeof(IOptions<BotOptions>));

        if (botOptionsObj == null)
        {
            string error = $"Could not fetch {typeof(IOptions<BotOptions>).Name}";

            object? discordErrorLoggerObj = ctx.Services.GetService(typeof(IDiscordErrorLogger));

            if (discordErrorLoggerObj == null)
            {
                throw new Exception($"Could not fetch {typeof(IDiscordErrorLogger).Name}");
            }

            var discordErrorLogger = (IDiscordErrorLogger)discordErrorLoggerObj;

            discordErrorLogger.LogError("WTF", error);

            return Task.FromResult(false);
        }

        BotOptions botOptions = ((IOptions<BotOptions>)botOptionsObj).Value;

        ulong guildId = botOptions.GuildId;

        DiscordChannel channel = ctx.Channel;

        if (IsPrivateMessageChannel(channel))
        {
            return Task.FromResult(false);
        }

        return !IsGuildChannel(channel, guildId) ? Task.FromResult(false) : Task.FromResult(true);
    }

    private bool IsGuildChannel(DiscordChannel channel, ulong botGuildId)
    {
        return channel.GuildId == botGuildId;
    }

    private bool IsPrivateMessageChannel(DiscordChannel channel)
    {
        return channel.IsPrivate;
    }
}
