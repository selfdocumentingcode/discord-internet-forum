namespace DifBot.Common.Models.Forums;

public class UnicodeEmojiReaction : PostReaction
{
    public string EmojiCodePoint { get; set; } = null!;

    public UnicodeEmoji Emoji { get; set; } = null!;
}
