using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MediatR;
using DifBot.Common.Models.Forums;
using DifBot.Config;
using DifBot.Data.Repositories;

namespace DifBot.CommandHandlers.Forum;

#pragma warning disable SA1402 // File may only contain a single type
public record PublishForumThreadCommand : BotCommandCommand
{
    public PublishForumThreadCommand(CommandContext ctx, DiscordThreadChannel threadChannel)
        : base(ctx)
    {
        ThreadChannel = threadChannel;
    }

    public DiscordThreadChannel ThreadChannel { get; }
}

public class PublishForumThreadHandler : IRequestHandler<PublishForumThreadCommand>
{
    private readonly ForumRepository _forumRepo;

    public PublishForumThreadHandler(ForumRepository forumRepo)
    {
        _forumRepo = forumRepo;
    }

    public async Task Handle(PublishForumThreadCommand request, CancellationToken cancellationToken)
    {
        var ctx = request.Ctx;

        var threadToPublish = await _forumRepo.GetFullForumThread(request.ThreadChannel.Id, cancellationToken);

        var serializedThread = JsonSerializer.Serialize(threadToPublish, JsonSerializerConfig.PublishOptions);

        var publishedThread = new PublishedThread
        {
            DiscordThreadId = request.ThreadChannel.Id,
            ForumChannelId = threadToPublish.ForumChannelId,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            ForumThreadJson = serializedThread,
        };

        await _forumRepo.CreateOrUpdatePublishedThread(publishedThread, cancellationToken);

        await ctx.Channel.SendMessageAsync($"Published thread \"{threadToPublish.OriginalPost!.Title}\" from \"{threadToPublish.ForumChannel.Name}\".");
    }
}
