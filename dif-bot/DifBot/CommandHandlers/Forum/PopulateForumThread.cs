using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MediatR;
using Microsoft.Extensions.Options;
using DifBot.Common.Models.Forums;
using DifBot.Config;
using DifBot.Data.Repositories;
using DifBot.Helpers;

namespace DifBot.CommandHandlers.Forum;

#pragma warning disable SA1402 // File may only contain a single type
public record PopulateForumThreadCommand : BotCommandCommand
{
    public PopulateForumThreadCommand(CommandContext ctx, DiscordThreadChannel threadChannel)
        : base(ctx)
    {
        ThreadChannel = threadChannel;
    }

    public DiscordThreadChannel ThreadChannel { get; }
}

public class PopulateForumThreadHandler : IRequestHandler<PopulateForumThreadCommand>
{
    private readonly ForumRepository _forumRepo;
    private readonly DiscordResolver _discordResolver;
    private readonly ForumOptions _forumOptions;

    public PopulateForumThreadHandler(ForumRepository forumRepo, DiscordResolver discordResolver, IOptions<ForumOptions> forumOptions)
    {
        _forumRepo = forumRepo;
        _discordResolver = discordResolver;
        _forumOptions = forumOptions.Value;
    }

    public async Task Handle(PopulateForumThreadCommand request, CancellationToken cancellationToken)
    {
        var ctx = request.Ctx;

        var dThread = request.ThreadChannel;

        if (dThread.Type != ChannelType.PublicThread)
        {
            await request.Ctx.RespondAsync("This command can only be used with thread channels.");
            return;
        }
        else if (dThread.Parent?.Type != ChannelType.GuildForum)
        {
            await request.Ctx.RespondAsync("Thread channel does not belong to a forum channel.");
            return;
        }

        var forumChannel = await _forumRepo.GetForumChannel(dThread.Parent.Id, cancellationToken)
            ?? throw new Exception($"Forum channel with id {dThread.Parent.Id} not found");

        await ctx.Channel.SendMessageAsync($"Populating thread \"{dThread.Name}\" from \"{forumChannel.Name}\".");

        if (dThread.MessageCount >= 50)
        {
            await ctx.Channel.SendMessageAsync($"Thread has 50 or more messages.");
        }

        var threadMessages = await dThread.GetUserSentMessages(1000);

        await ctx.Channel.SendMessageAsync($"Found {threadMessages.Count} messages in thread.");

        await CreateOrUpdateForumThread(dThread, forumChannel, cancellationToken);

        // Find active thread participants i.e. those who have sent messages in the thread
        var activeThreadParticipants = threadMessages
            .Select(x => x.Author)
            .DistinctBy(x => x.Id)
            .ToList();

        DiscordMember[] activeThreadMembers = (await Task.WhenAll(activeThreadParticipants.Select(x => _discordResolver.ResolveGuildMember(ctx.Guild, x.Id)))
                                    ?? throw new Exception("Could not resolve thread members. Result was null."))
                                    .Where(x => x is not null)
                                    .ToArray()!;

        await ctx.Channel.SendMessageAsync($"Found {activeThreadMembers.Length} active thread participants.");

        await CreateOrUpdateThreadMembers(activeThreadMembers, dThread.CreatorId, dThread.Id, cancellationToken);

        // The original message has the same id as the thread.
        var originalMessage = threadMessages.SingleOrDefault(x => x.Id == dThread.Id)
            ?? throw new Exception("Could not find original message");

        var otherThreadMessages = threadMessages
                                .Where(x => x.Id != dThread.Id)
                                .OrderBy(x => x.Id)
                                .ToList();

        await CreateOrUpdateMessages(originalMessage, otherThreadMessages, dThread, cancellationToken);

        var (attachmentCreatedCount, attachmentRemovedCount) = await UpdateMessageAttachments(threadMessages, cancellationToken);

        if (attachmentCreatedCount > 0)
        {
            await ctx.Channel.SendMessageAsync($"Created {attachmentCreatedCount} attachments.");
        }

        if (attachmentRemovedCount > 0)
        {
            await ctx.Channel.SendMessageAsync($"Removed {attachmentRemovedCount} attachments.");
        }

        var (reactionCreatedCount, reactionRemovedCount) = await UpdateMessageReactions(threadMessages, cancellationToken);

        if (reactionCreatedCount > 0)
        {
            await ctx.Channel.SendMessageAsync($"Created {reactionCreatedCount} reactions.");
        }

        if (reactionRemovedCount > 0)
        {
            await ctx.Channel.SendMessageAsync($"Removed {reactionRemovedCount} reactions.");
        }

        await ctx.Channel.SendMessageAsync($"Populated thread \"{dThread.Name}\" from \"{forumChannel.Name}\".");
    }

    private async Task CreateOrUpdateForumThread(DiscordThreadChannel dThread, ForumChannel forumChannel, CancellationToken cancellationToken)
    {
        var forumThread = new ForumThread
        {
            DiscordThreadId = dThread.Id,
            ForumChannelId = forumChannel.DiscordChannelId,
            DiscordThreadUrl = dThread.GetChannelUrl(),
            CreatedDate = dThread.CreationTimestamp.UtcDateTime,
            UpdatedDate = dThread.CreationTimestamp.UtcDateTime,
            InternalUpdatedDate = DateTime.UtcNow,
        };

        await _forumRepo.CreateOrUpdateForumThread(forumThread, cancellationToken);
    }

    private async Task CreateOrUpdateThreadMembers(DiscordMember[] dThreadMembers, ulong creatorId, ulong forumThreadId, CancellationToken cancellationToken)
    {
        var threadMembers = dThreadMembers.Select(x =>
        {
            var role = x.Roles.FirstOrDefault(x => _forumOptions.RelevantRoleIds.Contains(x.Id));

            return new ThreadMember
            {
                DiscordUserId = x.Id,
                ForumThreadId = forumThreadId,
                Name = x.DisplayName,
                AvatarUrl = x.GetAnyAvatarUrl(),
                Role = role?.Name,
                RoleColor = role?.Color.ToString(),
                IsThreadAuthor = x.Id == creatorId,
            };
        });

        foreach (var threadMember in threadMembers)
        {
            await _forumRepo.CreateOrUpdateThreadMember(threadMember, cancellationToken);
        }
    }

    private async Task CreateOrUpdateMessages(DiscordMessage originalMessage, IReadOnlyList<DiscordMessage> otherMessages, DiscordThreadChannel dThread, CancellationToken cancellationToken)
    {
        var forumOriginalPost = new ForumOriginalPost
        {
            DiscordMessageId = originalMessage.Id,
            ForumThreadId = dThread.Id,
            DiscordMessageUrl = originalMessage.JumpLink.ToString(),
            OriginalPostForumThreadId = dThread.Id,
            Author = new ThreadMember { DiscordUserId = originalMessage.Author.Id, ForumThreadId = dThread.Id },
            Content = originalMessage.Content,
            Title = dThread.Name,
        };

        await _forumRepo.CreateOrUpdateForumOriginalPost(forumOriginalPost, cancellationToken);

        var forumReplyMessages = otherMessages.Select(x =>
            new ForumReplyPost
            {
                DiscordMessageId = x.Id,
                ForumThreadId = dThread.Id,
                DiscordMessageUrl = x.JumpLink.ToString(),
                ReplyPostForumThreadId = dThread.Id,
                Author = new ThreadMember { DiscordUserId = x.Author.Id, ForumThreadId = dThread.Id },
                Content = x.Content,
                ReplyToId = x.ReferencedMessage?.Id,
                CreatedDate = x.CreationTimestamp.UtcDateTime,
                UpdatedDate = x.CreationTimestamp.UtcDateTime,
                InternalUpdatedDate = DateTime.UtcNow,
            });

        foreach (var item in forumReplyMessages)
        {
            await _forumRepo.CreateOrUpdateForumReplyPost(item, cancellationToken);
        }
    }

    private async Task<(int Created, int Removed)> UpdateMessageAttachments(IReadOnlyList<DiscordMessage> threadMessages, CancellationToken cancellationToken)
    {
        var createdAttachments = 0;
        var removedAttachments = 0;

        foreach (var message in threadMessages)
        {
            var attachments = message.Attachments.Select(x => new PostAttachment
            {
                DiscordAttachmentId = x.Id,
                ForumPostId = message.Id,
                Url = x.Url,
                Filename = x.FileName,
                ContentType = x.MediaType,
            }).ToArray();

            foreach (var attachment in attachments)
            {
                if (await _forumRepo.SavePostAttachment(attachment, cancellationToken))
                {
                    createdAttachments++;
                }
            }

            removedAttachments += await _forumRepo.RemoveOrphanedAttachments(message.Id, message.Attachments.Select(x => x.Id), cancellationToken);
        }

        return (createdAttachments, removedAttachments);
    }

    private async Task<(int Created, int Removed)> UpdateMessageReactions(IReadOnlyList<DiscordMessage> threadMessages, CancellationToken cancellationToken)
    {
        var createdReactions = 0;
        var removedReactions = 0;

        // TODO handle unicode emojis names
        foreach (var message in threadMessages)
        {
            var activeReactionIds = new List<ulong>();

            var discordEmojiReactions = message.Reactions
                .Where(x => x.Emoji.IsDiscordEmoji())
                .Select(x => new DiscordEmojiReaction
                {
                    DiscordEmojiId = x.Emoji.Id,
                    DiscordEmojiName = x.Emoji.Name,
                    ForumPostId = message.Id,
                    Count = x.Count,
                    Emoji = new Common.Models.Forums.DiscordEmoji
                    {
                        DiscordEmojiId = x.Emoji.Id,
                        DiscordEmojiName = x.Emoji.Name,
                        ImageUrl = x.Emoji.GetEmojiImageUrl(),
                    },
                }).ToArray();

            var unicodeEmojiReactions = message.Reactions
                .Where(x => !x.Emoji.IsDiscordEmoji())
                .Select(x => new UnicodeEmojiReaction
                {
                    EmojiCodePoint = x.Emoji.Name,
                    ForumPostId = message.Id,
                    Count = x.Count,
                    Emoji = new UnicodeEmoji
                    {
                        CodePoint = x.Emoji.Name,
                        ImageUrl = x.Emoji.GetEmojiImageUrl(),
                        EmojiName = "//TODO",
                    },
                }).ToArray();

            foreach (var reaction in discordEmojiReactions)
            {
                var (savedReaction, created) = await _forumRepo.SaveDiscordEmojiReaction(reaction, cancellationToken);

                activeReactionIds.Add(savedReaction.Id);

                if (created)
                {
                    createdReactions++;
                }
            }

            foreach (var reaction in unicodeEmojiReactions)
            {
                var (savedReaction, created) = await _forumRepo.SaveUnicodeEmojiReaction(reaction, cancellationToken);

                activeReactionIds.Add(savedReaction.Id);

                if (created)
                {
                    createdReactions++;
                }
            }

            removedReactions += await _forumRepo.RemoveOrphanedPostReactions(message.Id, activeReactionIds, cancellationToken);
        }

        return (createdReactions, removedReactions);
    }
}
