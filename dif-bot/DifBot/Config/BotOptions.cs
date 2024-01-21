namespace DifBot.Config;

public class BotOptions
{
    public const string OptionsKey = "DifBot";

    public string CommandPrefix { get; init; } = null!;

    public ulong GuildId { get; init; }

    /// <summary>
    /// Gets discord role with access to admin commands.
    /// </summary>
    public ulong AdminRoleId { get; init; }

    public ulong ErrorLogChannelId { get; init; }

    /// <summary>
    /// Gets a value indicating whether feature flag for populating message refs on Ready event.
    /// </summary>
    public bool AutoPopulateMessagesOnReadyEnabled { get; init; }
}
