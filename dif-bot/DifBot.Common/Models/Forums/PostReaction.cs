namespace DifBot.Common.Models.Forums;

public abstract class PostReaction
{
    public ulong Id { get; set; }

    public ulong ForumPostId { get; set; }

    public int Count { get; set; }

    public string PostReactionType { get; set; } = null!;
}
