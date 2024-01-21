namespace DifBot.Common.Models.Forums;

public class DiscordEmojiReaction : PostReaction
{
    public ulong DiscordEmojiId { get; set; }

    public string DiscordEmojiName { get; set; } = null!;

    public DiscordEmoji Emoji { get; set; } = null!;
}
