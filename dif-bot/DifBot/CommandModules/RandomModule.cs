using System.Threading.Tasks;
using DifBot.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DifBot.CommandModules
{
    [BaseCommandCheck]
    public class RandomModule : BaseCommandModule
    {
        [Command("oi")]
        public Task Oi(CommandContext ctx)
        {
            return ctx.RespondAsync("Oi!");
        }

        [Command("slap")]
        public Task Slap(CommandContext ctx, DiscordMember member)
        {
            var slapper = ctx.Member?.DisplayName ?? "Someone";
            var slapee = member.DisplayName;

            return ctx.RespondAsync($"{slapper} slaps {slapee} around a bit with a large trout");
        }
    }
}
