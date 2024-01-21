using System.Collections.Generic;
using System.Threading.Tasks;
using DifBot.Attributes;
using DifBot.CommandHandlers;
using DifBot.CommandHandlers.Forum;
using DifBot.Common.Extensions;
using DifBot.Data.Repositories;
using DifBot.Workers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;

namespace DifBot.CommandModules;

[BaseCommandCheck]
[RequireOwnerOrAdmin]
[Group("admin")]
[ModuleLifespan(ModuleLifespan.Transient)]
public class AdminModule : BaseCommandModule
{
    private readonly CommandQueueChannel _commandQueue;

    public AdminModule(
        CommandQueueChannel commandQueue)
    {
        _commandQueue = commandQueue;
    }

    [Command("nickname")]
    public Task ChangeNickname(CommandContext ctx, [RemainingText] string name)
    {
        name = name.RemoveQuotes();

        return ctx.Guild.CurrentMember.ModifyAsync((props) =>
        {
            props.Nickname = name;
        });
    }

    [Command("populate-messages")]
    public Task PopulateMessages(CommandContext ctx)
    {
        return _commandQueue.Writer.WriteAsync(new PopulateMessagesCommand(ctx)).AsTask();
    }

    [Command("create-forum")]
    public async Task CreateForum(CommandContext ctx, DiscordChannel channel, [RemainingText] string displayName)
    {
        await _commandQueue.Writer.WriteAsync(new CreateForumChannelCommand(ctx, channel, displayName));
    }

    [Command("populate-thread")]
    public async Task PopulateThread(CommandContext ctx, DiscordThreadChannel channel)
    {
        await _commandQueue.Writer.WriteAsync(new PopulateForumThreadCommand(ctx, channel));
    }

    [Command("publish-forum")]
    public async Task PublishForum(CommandContext ctx, DiscordChannel channel)
    {
        await _commandQueue.Writer.WriteAsync(new PublishForumChannelCommand(ctx, channel));
    }

    [Command("publish-thread")]
    public async Task PublishThread(CommandContext ctx, DiscordThreadChannel channel)
    {
        await _commandQueue.Writer.WriteAsync(new PublishForumThreadCommand(ctx, channel));
    }

    [Command("publish-fake-threads")]
    public async Task PublishFakeThread(CommandContext ctx, DiscordChannel channel)
    {
        await _commandQueue.Writer.WriteAsync(new PublishFakeForumThreadCommand(ctx, channel));
    }
}
