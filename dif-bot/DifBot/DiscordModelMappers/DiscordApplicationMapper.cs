using DSharpPlus.Entities;
using DifBot.Common.AppDiscordModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DifBot.DiscordModelMappers
{
    public static class DiscordApplicationMapper
    {
        public static AppDiscordApplication Map(DiscordApplication discordApplication)
        {
            var appDiscordApplication = new AppDiscordApplication();

            appDiscordApplication.Owners = discordApplication.Owners.Select(DiscordUserMapper.Map);

            return appDiscordApplication;
        }
    }
}
