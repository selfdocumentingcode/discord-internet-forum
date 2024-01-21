using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DifBot.Common.Models.Forums;

namespace DifBot.Data.Repositories;

public class ForumRepository : IDisposable
{
    private readonly BotDbContext _dbContext;
    private bool _disposed;

    public ForumRepository(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ForumChannel?> GetForumChannel(ulong discordChannelId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ForumChannels.FindEntityAsync(discordChannelId, cancellationToken);
    }

    public async Task<ForumChannel> CreateOrUpdateForumChannel(ForumChannel forumChannel, CancellationToken cancellationToken = default)
    {
        var storedChannel = await _dbContext.ForumChannels.FindEntityAsync(forumChannel.DiscordChannelId, cancellationToken);

        if (storedChannel == null)
        {
            _dbContext.Add(forumChannel);
        }
        else
        {
            storedChannel.Name = forumChannel.Name;
            storedChannel.DisplayName = forumChannel.DisplayName;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return forumChannel;
    }

    public async Task CreateOrUpdateForumThread(ForumThread forumThread, CancellationToken cancellationToken = default)
    {
        var storedThread = await _dbContext.ForumThreads.FindEntityAsync(forumThread.DiscordThreadId, cancellationToken);

        if (storedThread == null)
        {
            _dbContext.Add(forumThread);
        }
        else
        {
            storedThread.UpdatedDate = forumThread.UpdatedDate;
            storedThread.InternalUpdatedDate = forumThread.InternalUpdatedDate;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateOrUpdateThreadMember(ThreadMember threadMember, CancellationToken cancellationToken = default)
    {
        var storedThreadMember = await _dbContext.ThreadMembers.FindEntityAsync(threadMember.DiscordUserId, threadMember.ForumThreadId, cancellationToken);

        if (storedThreadMember == null)
        {
            _dbContext.Add(threadMember);
        }
        else
        {
            storedThreadMember.Name = threadMember.Name;
            storedThreadMember.AvatarUrl = threadMember.AvatarUrl;
            storedThreadMember.Role = threadMember.Role;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateOrUpdateForumOriginalPost(ForumOriginalPost forumOriginalPost, CancellationToken cancellationToken = default)
    {
        var storedAuthor = await _dbContext.ThreadMembers.FindEntityAsync(forumOriginalPost.Author.DiscordUserId, forumOriginalPost.ForumThreadId, cancellationToken)
            ?? throw new Exception("Author must be created before creating a post.");

        var storedOriginalPost = await _dbContext.ForumOriginalPosts.FindEntityAsync(forumOriginalPost.DiscordMessageId, cancellationToken);

        forumOriginalPost.Author = storedAuthor;

        if (storedOriginalPost == null)
        {
            _dbContext.Add(forumOriginalPost);
        }
        else
        {
            storedOriginalPost.Title = forumOriginalPost.Title;
            storedOriginalPost.Content = forumOriginalPost.Content;
            storedOriginalPost.Author = forumOriginalPost.Author;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateOrUpdateForumReplyPost(ForumReplyPost forumReplyPost, CancellationToken cancellationToken = default)
    {
        var storedAuthor = await _dbContext.ThreadMembers.FindEntityAsync(forumReplyPost.Author.DiscordUserId, forumReplyPost.ForumThreadId, cancellationToken)
            ?? throw new Exception("Author must be created before creating a post.");

        var storedReplyPost = await _dbContext.ForumReplyPosts.FindEntityAsync(forumReplyPost.DiscordMessageId, cancellationToken);

        forumReplyPost.Author = storedAuthor;

        if (storedReplyPost == null)
        {
            _dbContext.Add(forumReplyPost);
        }
        else
        {
            storedReplyPost.Content = forumReplyPost.Content;
            storedReplyPost.Author = forumReplyPost.Author;
            storedReplyPost.ReplyToId = forumReplyPost.ReplyToId;
            storedReplyPost.CreatedDate = forumReplyPost.CreatedDate;
            storedReplyPost.UpdatedDate = forumReplyPost.UpdatedDate;
            storedReplyPost.InternalUpdatedDate = forumReplyPost.InternalUpdatedDate;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SavePostAttachment(PostAttachment attachment, CancellationToken cancellationToken)
    {
        var alreadyExists = await _dbContext.PostAttachments.AnyAsync(x => x.DiscordAttachmentId == attachment.DiscordAttachmentId, cancellationToken);

        if (alreadyExists)
        {
            return false;
        }

        _dbContext.Add(attachment);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int> RemoveOrphanedAttachments(ulong discordMessageId, IEnumerable<ulong> activeAttachmentIds, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PostAttachments
            .Where(x => x.ForumPostId == discordMessageId && !activeAttachmentIds.Contains(x.DiscordAttachmentId))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<(DiscordEmojiReaction Reaction, bool Created)> SaveDiscordEmojiReaction(DiscordEmojiReaction reaction, CancellationToken cancellationToken = default)
    {
        var storedReaction = await _dbContext.DiscordEmojiReactions
                            .Include(x => x.Emoji)
                            .SingleOrDefaultAsync(
                                x => x.DiscordEmojiId == reaction.DiscordEmojiId
                                    && x.DiscordEmojiName == reaction.DiscordEmojiName
                                    && x.ForumPostId == reaction.ForumPostId,
                                cancellationToken);

        if (storedReaction is not null)
        {
            storedReaction.Count = reaction.Count;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (storedReaction, false);
        }
        else
        {
            var storedEmoji = await _dbContext.DiscordEmojis
                            .SingleOrDefaultAsync(x => x.DiscordEmojiId == reaction.DiscordEmojiId, cancellationToken);

            if (storedEmoji is not null)
            {
                reaction.Emoji = storedEmoji;
            }

            _dbContext.Add(reaction);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (reaction, true);
        }
    }

    public async Task<(UnicodeEmojiReaction Reaction, bool Created)> SaveUnicodeEmojiReaction(UnicodeEmojiReaction reaction, CancellationToken cancellationToken = default)
    {
        var storedReaction = await _dbContext.UnicodeEmojiReactions
                            .SingleOrDefaultAsync(
                                x => x.EmojiCodePoint == reaction.EmojiCodePoint
                                && x.ForumPostId == reaction.ForumPostId,
                                cancellationToken);

        if (storedReaction is not null)
        {
            storedReaction.Count = reaction.Count;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (storedReaction, false);
        }
        else
        {
            var storedEmoji = await _dbContext.UnicodeEmojis
                                .SingleOrDefaultAsync(x => x.CodePoint == reaction.EmojiCodePoint, cancellationToken);

            if (storedEmoji is not null)
            {
                reaction.Emoji = storedEmoji;
            }

            _dbContext.Add(reaction);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (reaction, true);
        }
    }

    public async Task<int> RemoveOrphanedPostReactions(ulong discordMessageId, IEnumerable<ulong> activePostReactionIds, CancellationToken cancellationToken)
    {
        return await _dbContext.PostReactions
            .Where(x => x.ForumPostId == discordMessageId && !activePostReactionIds.Contains(x.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<ForumThread> GetFullForumThread(ulong discordThreadId, CancellationToken cancellationToken = default)
    {
        var thread = await _dbContext.ForumThreads
            .Include(x => x.ForumChannel)
            .Include(x => x.OriginalPost).ThenInclude(x => x!.Author)
            .Include(x => x.OriginalPost).ThenInclude(x => x!.Attachments)
            .Include(x => x.OriginalPost).ThenInclude(x => x!.Reactions)
            .Include(x => x.ReplyPosts).ThenInclude(x => x!.Author)
            .Include(x => x.ReplyPosts).ThenInclude(x => x!.Attachments)
            .Include(x => x.ReplyPosts).ThenInclude(x => x!.Reactions)
            .Include(x => x.Members)
            .SingleOrDefaultAsync(x => x.DiscordThreadId == discordThreadId, cancellationToken)
            ?? throw new Exception($"Thread with DiscordThreadId {discordThreadId} not found.");

        // Load emojis for all posts in the thread and let EF Core fix up the navigation properties
        ulong[] messageIdsToLoadEmojisFor = thread.ReplyPosts
                                        .Select(x => x.DiscordMessageId)
                                        .ToArray()
                                        .Append(thread.OriginalPost!.DiscordMessageId)
                                        .ToArray()
                                        ?? Array.Empty<ulong>();

        var discordReactionsWithEmoji = await _dbContext.Set<DiscordEmojiReaction>()
                                        .Include(r => r.Emoji)
                                        .Where(r => messageIdsToLoadEmojisFor.Contains(r.ForumPostId))
                                        .ToListAsync(cancellationToken);

        var unicodeReactionsWithEmoji = await _dbContext.Set<UnicodeEmojiReaction>()
                                        .Include(r => r.Emoji)
                                        .Where(r => messageIdsToLoadEmojisFor.Contains(r.ForumPostId))
                                        .ToListAsync(cancellationToken);

        return thread;
    }

    public async Task<PublishedThread> CreateOrUpdatePublishedThread(PublishedThread publishedThread, CancellationToken cancellationToken = default)
    {
        var storedPublishedThread = await _dbContext.PublishedThreads
                                    .FindEntityAsync(publishedThread.DiscordThreadId, cancellationToken);

        if (storedPublishedThread == null)
        {
            _dbContext.Add(publishedThread);
        }
        else
        {
            storedPublishedThread.UpdatedDate = publishedThread.UpdatedDate;
            storedPublishedThread.ForumThreadJson = publishedThread.ForumThreadJson;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return publishedThread;
    }

    public async Task<PublishedForum> CreateOrUpdatePublishedForum(PublishedForum publishedForum, CancellationToken cancellationToken = default)
    {
        var storedPublishedForum = await _dbContext.PublishedForums
                                    .FindEntityAsync(publishedForum.DiscordForumId, cancellationToken);

        if (storedPublishedForum == null)
        {
            _dbContext.Add(publishedForum);
        }
        else
        {
            storedPublishedForum.UpdatedDate = publishedForum.UpdatedDate;
            storedPublishedForum.ForumChannelJson = publishedForum.ForumChannelJson;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return publishedForum;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _dbContext.Dispose();
        }

        _disposed = true;
    }
}
