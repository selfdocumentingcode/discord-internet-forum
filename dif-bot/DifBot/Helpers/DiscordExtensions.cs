using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DifBot.Helpers;

public static class DiscordExtensions
{
    public static string GetAnyAvatarUrl(this DiscordMember member)
    {
        return member.GuildAvatarUrl ?? member.AvatarUrl ?? member.DefaultAvatarUrl;
    }

    /// <summary>
    /// Gets all regular and reply messages sent by users in a thread.
    /// </summary>
    /// <param name="thread">The thread channel to fetch the messages from.</param>
    /// <param name="limit">Maximum number of messages to fetch.</param>
    /// <returns><see cref="List{DiscordMessage}"/>.</returns>
    public static async Task<List<DiscordMessage>> GetUserSentMessages(this DiscordThreadChannel thread, int limit = 100)
    {
        var messages = new List<DiscordMessage>();

        await foreach (var message in thread.GetMessagesAsync(limit))
        {
            if ((message.Author?.IsSystem ?? false) || (message.Author?.IsBot ?? false)
                || message.MessageType != MessageType.Default && message.MessageType != MessageType.Reply)
            {
                continue;
            }

            // work around for library bug returning the same message multiple times
            if (messages.Any(x => x.Id == message.Id))
            {
                break;
            }

            messages.Add(message);
        }

        return messages;
    }

    public static bool IsDiscordEmoji(this DiscordEmoji emoji)
    {
        return emoji.Id != 0;
    }

    public static string GetEmojiImageUrl(this DiscordEmoji emoji)
    {
        bool isEmote = emoji.Id != 0;

        return isEmote ? emoji.Url : EmojiHelper.GetExternalEmojiImageUrl(emoji.Name);
    }
}
