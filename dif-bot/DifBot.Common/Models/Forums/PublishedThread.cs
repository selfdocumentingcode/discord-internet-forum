using System;

namespace DifBot.Common.Models.Forums;

public class PublishedThread
{
    public ulong DiscordThreadId { get; set; }

    public ulong ForumChannelId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public string ForumThreadJson { get; set; } = null!;
}
