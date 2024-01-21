using System;

namespace DifBot.Common.Models.Forums;

public class PublishedForum
{
    public ulong DiscordForumId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public string ForumChannelJson { get; set; } = null!;
}

