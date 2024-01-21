using System;
using System.Collections.Generic;

namespace DifBot.Common.Models.Forums;

public class ForumThread : IPublishable
{
    public ulong DiscordThreadId { get; set; }

    public ulong ForumChannelId { get; set; }

    public string DiscordThreadUrl { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public DateTime InternalUpdatedDate { get; set; }

    public DateTime? PublishDate { get; set; }

    public bool IsPublished { get; set; }

    public virtual ForumChannel ForumChannel { get; set; } = null!;

    public virtual ForumOriginalPost? OriginalPost { get; set; }

    public virtual ICollection<ForumReplyPost> ReplyPosts { get; set; } = null!;

    public virtual ICollection<ThreadMember> Members { get; set; } = null!;
}
