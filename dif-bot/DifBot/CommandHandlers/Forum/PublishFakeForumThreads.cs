using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MediatR;
using DifBot.Common.Models.Forums;
using DifBot.Config;
using DifBot.Data.Repositories;
using DifBot.Helpers;

namespace DifBot.CommandHandlers.Forum;

#pragma warning disable SA1402 // File may only contain a single type
public record PublishFakeForumThreadCommand : BotCommandCommand
{
    public PublishFakeForumThreadCommand(CommandContext ctx, DiscordChannel forumChannel)
        : base(ctx)
    {
        ForumChannel = forumChannel;
    }

    public DiscordChannel ForumChannel { get; }
}

public class PublishFakeForumThreads : IRequestHandler<PublishFakeForumThreadCommand>
{
    private static readonly Regex FakeTitleRegex = new(@"\[fake\]");
    private static readonly Regex FakeUserRegex = new(@"\[fake user (\d+)\]");
    private static readonly Regex FakeUserMentionRegex = new(@"\[@fake user (\d+)\]");

    private readonly ForumRepository _forumRepo;
    private readonly ForumRepositoryFactory _forumRepoFactory;

    public PublishFakeForumThreads(ForumRepository forumRepo, ForumRepositoryFactory forumRepoFactory)
    {
        _forumRepo = forumRepo;
        _forumRepoFactory = forumRepoFactory;
    }

    public async Task Handle(PublishFakeForumThreadCommand request, CancellationToken cancellationToken)
    {
        var ctx = request.Ctx;

        if (request.ForumChannel.Type != ChannelType.GuildForum)
        {
            await request.Ctx.RespondAsync("The channel is not a forum channel.");
            return;
        }

        var dForumChannel = (DiscordForumChannel)request.ForumChannel;

        var fakeThreads = dForumChannel.Threads.Where(t => FakeTitleRegex.IsMatch(t.Name)).ToList();

        await ctx.Channel.SendMessageAsync($"Publishing {fakeThreads.Count} fake threads... from **{dForumChannel.Name}**");

        foreach (var fakeThread in fakeThreads)
        {
            var threadToPublish = await _forumRepo.GetFullForumThread(fakeThread.Id, cancellationToken);

            threadToPublish.OriginalPost!.Title = FakeTitleRegex.Replace(threadToPublish.OriginalPost!.Title, string.Empty);

            FakeOriginalPostAuthor(fakeThread.Id, threadToPublish.OriginalPost!);
            FakeMessageReactionCount(threadToPublish.OriginalPost!);

            var fakePostAuthorName = threadToPublish.OriginalPost!.Author.Name;

            foreach (var replyPost in threadToPublish.ReplyPosts)
            {
                FakePostAuthor(fakeThread.Id, replyPost, fakePostAuthorName!);
                FakeMessageReactionCount(replyPost);
            }

            threadToPublish.Members = Enumerable.Empty<ThreadMember>().ToList();

            var serializedThread = JsonSerializer.Serialize(threadToPublish, JsonSerializerConfig.PublishOptions);

            var publishedThread = new PublishedThread
            {
                DiscordThreadId = fakeThread.Id,
                ForumChannelId = threadToPublish.ForumChannelId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                ForumThreadJson = serializedThread,
            };

            using (var forumRepo = _forumRepoFactory.CreateRepository())
            {
                await forumRepo.CreateOrUpdatePublishedThread(publishedThread, cancellationToken);
            }

            await ctx.Channel.SendMessageAsync($"Published fake thread **{threadToPublish.OriginalPost!.Title}**.");
        }
    }

    private static void FakeOriginalPostAuthor(ulong threadId, ForumPost post)
    {
        var fakeUserId = FakeUserRegex.Match(post.Content).Groups[1].Value
                        ?? throw new Exception($"Could not find fake user id in message content: {post.Content}");

        // remove fake user id marker from message content
        post.Content = FakeUserRegex.Replace(post.Content, string.Empty);

        var fakeUserSeed = GetFakeUserSeed(threadId, fakeUserId);
        var fakeUserName = PseudonymGenerator.GetRandomPseudonym(fakeUserSeed);
        var (fakeRole, fakeRoleColor) = PseudonymGenerator.GetRandomRole(fakeUserSeed);

        post.Author = new ThreadMember()
        {
            DiscordUserId = post.Author.DiscordUserId,
            AvatarUrl = string.Empty,
            Name = fakeUserName,
            Role = fakeRole,
            RoleColor = fakeRoleColor,
            ForumThreadId = post.Author.ForumThreadId,
            IsThreadAuthor = true,
        };
    }

    private static void FakePostAuthor(ulong threadId, ForumPost post, string fakePostAuthorName)
    {
        var fakeUserId = FakeUserRegex.Match(post.Content).Groups[1].Value
                        ?? throw new Exception($"Could not find fake user id in message content: {post.Content}");

        // remove fake user id marker from message content
        post.Content = FakeUserRegex.Replace(post.Content, string.Empty);

        var fakeUserSeed = GetFakeUserSeed(threadId, fakeUserId);
        var fakeUserName = PseudonymGenerator.GetRandomPseudonym(fakeUserSeed);
        var (fakeRole, fakeRoleColor) = PseudonymGenerator.GetRandomRole(fakeUserSeed);

        post.Author = new ThreadMember()
        {
            DiscordUserId = post.Author.DiscordUserId,
            AvatarUrl = string.Empty,
            Name = fakeUserName,
            Role = fakeRole,
            RoleColor = fakeRoleColor,
            ForumThreadId = post.Author.ForumThreadId,
            IsThreadAuthor = fakePostAuthorName == fakeUserName,
        };

        var fakeUserMentionIds = FakeUserMentionRegex.Matches(post.Content).Select(m => m.Groups[1].Value).ToList();

        foreach (var fakeUserMentionId in fakeUserMentionIds)
        {
            var fakeUserMentionSeed = GetFakeUserSeed(threadId, fakeUserMentionId);
            var fakeUserMentionName = PseudonymGenerator.GetRandomPseudonym(fakeUserMentionSeed);

            post.Content = FakeUserMentionRegex.Replace(post.Content, $"@{fakeUserMentionName}");
        }
    }

    private static void FakeMessageReactionCount(ForumPost originalPost)
    {
        foreach (var reaction in originalPost.Reactions)
        {
            var messageSeed = GetMessageReactionSeed(originalPost.DiscordMessageId, reaction.Id);

            var randomGenerator = new Random(messageSeed);

            reaction.Count = randomGenerator.Next(1, 11);
        }
    }

    private static int GetFakeUserSeed(ulong threadId, string fakeUserId)
    {
        unchecked
        {
            return (int)(threadId ^ 13 ^ ulong.Parse(fakeUserId));
        }
    }

    private static int GetMessageReactionSeed(ulong messageId, ulong reactionId)
    {
        unchecked
        {
            return (int)(messageId ^ 13 ^ reactionId);
        }
    }
}
