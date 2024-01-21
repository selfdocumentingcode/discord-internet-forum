namespace DifBot.Common.Models.Forums;

public class DiscordEmoji
{
    public ulong DiscordEmojiId { get; set; }

    public string DiscordEmojiName { get; set; } = null!;

    public string? ImageUrl { get; set; }
}
