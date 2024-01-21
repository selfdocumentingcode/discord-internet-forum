﻿using System;
using System.Threading;
using System.Threading.Tasks;
using DifBot.CommandHandlers;
using DifBot.Services.Loggers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DifBot.Infrastructure;

public class CommandRequestPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<CommandRequestPipelineBehaviour<TRequest, TResponse>> _logger;
    private readonly IDiscordErrorLogger _discordErrorLogger;

    public CommandRequestPipelineBehaviour(
        ILogger<CommandRequestPipelineBehaviour<TRequest, TResponse>> logger,
        IDiscordErrorLogger discordErrorLogger)
    {
        _logger = logger;
        _discordErrorLogger = discordErrorLogger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (Exception ex) when (request is BotCommandCommand commandCommand)
        {
            await HandeCommandCommandException(commandCommand, ex);
            return default!;
        }
        catch (Exception ex) when (request is IRequest command)
        {
            HandleRequestException(command, ex);
            return default!;
        }
    }

    private void HandleRequestException(IRequest request, Exception ex)
    {
        _discordErrorLogger.LogError(ex.Message);

        _logger.LogError(ex, nameof(HandeCommandCommandException));
    }

    private async Task HandeCommandCommandException(BotCommandCommand request, Exception ex)
    {
        var ctx = request.Ctx;

        await ctx.RespondAsync(ex.Message);

        _discordErrorLogger.LogCommandError(ctx, ex.ToString());

        _logger.LogError(ex, nameof(HandeCommandCommandException));
    }
}
