using System.Threading.Tasks;
using DSharpPlus.SlashCommands;

namespace DifBot.CommandModules;

public class GlobalSlashModule : ApplicationCommandModule
{
    [SlashCommand("oi", "Oi!")]
    public Task Oi(InteractionContext ctx)
    {
        return ctx.CreateResponseAsync("Oi!");
    }
}
