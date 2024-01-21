using System.Collections.Generic;

namespace DifBot.Common.Models.Forums;

public abstract class ForumPost
{
    public ulong DiscordMessageId { get; set; }

    public ulong ForumThreadId { get; set; }

    public string DiscordMessageUrl { get; set; } = null!;

    public ThreadMember Author { get; set; } = null!;

    public string Content { get; set; } = null!;

    public IEnumerable<PostReaction> Reactions { get; set; } = null!;

    public IEnumerable<PostAttachment> Attachments { get; set; } = null!;
}
