using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MediatR;
using DifBot.Common.Models.Forums;
using DifBot.Common.Models.Forums.Converters;
using DifBot.Config;
using DifBot.Data.Repositories;
using DifBot.Helpers;

namespace DifBot.CommandHandlers.Forum;

#pragma warning disable SA1402 // File may only contain a single type
public record PublishForumChannelCommand : BotCommandCommand
{
    public PublishForumChannelCommand(CommandContext ctx, DiscordChannel forumChannel)
        : base(ctx)
    {
        ForumChannel = forumChannel;
    }

    public DiscordChannel ForumChannel { get; }
}

public class PublishForumChannelHandler : IRequestHandler<PublishForumChannelCommand>
{
    private readonly ForumRepository _forumRepo;

    public PublishForumChannelHandler(ForumRepository forumRepo)
    {
        _forumRepo = forumRepo;
    }

    public async Task Handle(PublishForumChannelCommand request, CancellationToken cancellationToken)
    {
        var ctx = request.Ctx;
        var channel = request.ForumChannel;

        if (channel.Type != ChannelType.GuildForum)
        {
            await request.Ctx.RespondAsync("The channel is not a forum channel.");
            return;
        }

        var forumChannel = await _forumRepo.GetForumChannel(channel.Id, cancellationToken)
            ?? throw new Exception($"Forum channel with id {channel.Id} doesn't exist.");

        var serializedForum = JsonSerializer.Serialize(forumChannel, JsonSerializerConfig.PublishOptions);

        var publishedForum = new PublishedForum
        {
            DiscordForumId = forumChannel.DiscordChannelId,
            ForumChannelJson = serializedForum,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
        };

        await _forumRepo.CreateOrUpdatePublishedForum(publishedForum, cancellationToken);

        await ctx.Channel.SendMessageAsync($"Published channel \"{forumChannel.DisplayName}\" ({forumChannel.Name}).");
    }
}
