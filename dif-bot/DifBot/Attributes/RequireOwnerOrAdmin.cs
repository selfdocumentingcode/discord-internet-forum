using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using DifBot.Common.AppDiscordModels;
using DifBot.DiscordModelMappers;
using DifBot.Services;
using System.Linq;
using DSharpPlus.Entities;
using DifBot.Config;
using DifBot.Services.Loggers;
using Microsoft.Extensions.Options;

namespace DifBot.Attributes;

public class RequireOwnerOrAdmin : CheckBaseAttribute
{
    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        object? botOptionsObj = ctx.Services.GetService(typeof(IOptions<BotOptions>));

        if (botOptionsObj == null)
        {
            string error = $"Could not fetch {typeof(IOptions<BotOptions>).Name}";

            object? discordErrorLoggerObj = ctx.Services.GetService(typeof(DiscordErrorLogger));

            if (discordErrorLoggerObj == null)
            {
                throw new Exception($"Could not fetch {typeof(DiscordErrorLogger).Name}");
            }

            var discordErrorLogger = (DiscordErrorLogger)discordErrorLoggerObj;

            discordErrorLogger.LogError(error);

            return Task.FromResult(false);
        }

        BotOptions botOptions = ((IOptions<BotOptions>)botOptionsObj).Value;

        var currentMember = ctx.Member;
        var discordApplication = ctx.Client.CurrentApplication;

        if (currentMember is null)
        {
            return Task.FromResult(false);
        }

        var isBotOwner = discordApplication?.Owners?.Any(x => x.Id == currentMember.Id) ?? false;

        if (isBotOwner)
            return Task.FromResult(true);

        var adminRoleId = botOptions.AdminRoleId;

        var currentUserIsAdmin = currentMember.Roles.Select(r => r.Id).Contains(adminRoleId);

        return Task.FromResult(currentUserIsAdmin);
    }
}
