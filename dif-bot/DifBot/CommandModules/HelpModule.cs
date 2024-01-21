using System.Text;
using System.Threading.Tasks;
using DifBot.Attributes;
using DifBot.Config;
using DifBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Options;

namespace DifBot.CommandModules;

[BaseCommandCheck]
[Group("help")]
public class HelpModule : BaseCommandModule
{
    private readonly BotOptions _options;

    public HelpModule(
        IOptions<BotOptions> options)
    {
        _options = options.Value;
    }

    [GroupCommand]
    public Task Help(CommandContext ctx)
    {
        var sb = new StringBuilder();

        var commandPrefix = _options.CommandPrefix;

        sb.AppendLine($"`{commandPrefix}admin nickname [name]`");
        sb.AppendLine($"`   Change Bot's nickname`");
        sb.AppendLine();

        sb.AppendLine("More in source code: [github.com/selfdocumentingcode/discord-internet-forum](https://github.com/selfdocumentingcode/discord-internet-forum)");

        var eb = EmbedBuilderHelper.BuildSimpleEmbed("Help", sb.ToString());

        return ctx.RespondAsync(eb);
    }
}
