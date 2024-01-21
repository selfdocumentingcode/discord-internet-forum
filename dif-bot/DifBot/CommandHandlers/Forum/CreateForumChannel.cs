using System;
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
public record CreateForumChannelCommand : BotCommandCommand
{
    public CreateForumChannelCommand(CommandContext ctx, DiscordChannel forumChannel, string displayName)
        : base(ctx)
    {
        ForumChannel = forumChannel;
        DisplayName = displayName;
    }

    public DiscordChannel ForumChannel { get; }

    public string DisplayName { get; }
}

public class CreateForumChannelHandler : IRequestHandler<CreateForumChannelCommand>
{
    private readonly ForumRepository _forumRepo;

    public CreateForumChannelHandler(ForumRepository forumRepo)
    {
        _forumRepo = forumRepo;
    }

    public async Task Handle(CreateForumChannelCommand request, CancellationToken cancellationToken)
    {
        var channel = request.ForumChannel;
        var displayName = request.DisplayName;

        if (channel.Type != ChannelType.GuildForum)
        {
            await request.Ctx.RespondAsync("The channel is not a forum channel.");
            return;
        }

        var forumChannel = new ForumChannel
        {
            DiscordChannelId = channel.Id,
            Name = channel.Name,
            DisplayName = displayName,
            DiscordChannelUrl = channel.GetChannelUrl(),
        };

        await _forumRepo.CreateOrUpdateForumChannel(forumChannel, cancellationToken);

        await request.Ctx.RespondAsync($"Forum channel {displayName} ({channel.Name}) created.");
    }
}
