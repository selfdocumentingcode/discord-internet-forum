namespace DifBot.Common.Models.Forums;

public class UnicodeEmoji
{
    public string CodePoint { get; set; } = null!;

    public string EmojiName { get; set; } = null!;

    public string? ImageUrl { get; set; }
}
