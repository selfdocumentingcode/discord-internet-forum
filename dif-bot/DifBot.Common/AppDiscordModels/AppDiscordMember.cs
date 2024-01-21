using System.Collections.Generic;

namespace DifBot.Common.AppDiscordModels;

public class AppDiscordMember : AppDiscordUser
{
    public string Nickname { get; set; } = string.Empty;
    public IEnumerable<AppDiscordRole> Roles { get; set; } = null!;
}
