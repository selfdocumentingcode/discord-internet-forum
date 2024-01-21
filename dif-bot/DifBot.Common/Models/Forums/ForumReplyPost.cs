using System;

namespace DifBot.Common.Models.Forums;

public class ForumReplyPost : ForumPost, IPublishable
{
    public ulong? ReplyToId { get; set; }

    public virtual ForumPost? ReplyTo { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public DateTime InternalUpdatedDate { get; set; }

    public DateTime? PublishDate { get; set; }

    public bool IsPublished { get; set; }

    public ulong ReplyPostForumThreadId { get; set; }
}
