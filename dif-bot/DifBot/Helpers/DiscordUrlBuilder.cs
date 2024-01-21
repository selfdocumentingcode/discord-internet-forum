using System;
using DSharpPlus.Entities;

namespace DifBot.Helpers;

public static class DiscordUrlBuilder
{
    private const string BaseDiscordUrl = "https://discord.com/";
    private const string DiscordChannelLinkFormat = "channels/{0}/{1}";

    public static string GetChannelUrl(ulong guildId, ulong channelId)
    {
        return $"{BaseDiscordUrl}{string.Format(DiscordChannelLinkFormat, guildId, channelId)}";
    }

    public static string GetChannelUrl(this DiscordChannel channel)
    {
        return GetChannelUrl(channel.GuildId ?? throw new Exception("Channel is missing guildId"), channel.Id);
    }
}
