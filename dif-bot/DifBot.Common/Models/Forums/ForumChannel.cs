using System;

namespace DifBot.Common.Models.Forums;

public class ForumChannel
{
    public ulong DiscordChannelId { get; set; }

    public string Name { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string DiscordChannelUrl { get; set; } = null!;
}
