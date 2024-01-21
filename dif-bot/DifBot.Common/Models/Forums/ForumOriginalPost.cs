namespace DifBot.Common.Models.Forums;

public class ForumOriginalPost : ForumPost
{
    public string Title { get; set; } = null!;

    public ulong OriginalPostForumThreadId { get; set; }
}
