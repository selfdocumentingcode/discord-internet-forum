using DSharpPlus.Entities;
using DifBot.Common.AppDiscordModels;

namespace DifBot.DiscordModelMappers;
public static class DiscordChannelMapper
{
    public static AppDiscordChannel Map(DiscordChannel discordChannel)
    {
        var channel = new AppDiscordChannel();

        channel.Id = discordChannel.Id;
        channel.Name = discordChannel.Name;

        return channel;
    }
}
