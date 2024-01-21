using System;

namespace DifBot.Common.Models.Forums;

public class ThreadMember
{
    public ulong DiscordUserId { get; set; }

    public ulong ForumThreadId { get; set; }

    public string? Name { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Role { get; set; }

    public string? RoleColor { get; set; }

    public EnumLookup<ConsentType>? ConsentType { get; set; }

    public DateTime? ConsentDate { get; set; }

    public bool IsThreadAuthor { get; set; }
}
