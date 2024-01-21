namespace DifBot.Common.Models.Forums;

public class PostAttachment
{
    public ulong DiscordAttachmentId { get; set; }

    public ulong ForumPostId { get; set; }

    public string Url { get; set; } = null!;

    public string Filename { get; set; } = null!;

    public string ContentType { get; set; } = null!;
}
