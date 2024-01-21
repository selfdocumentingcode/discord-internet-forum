using System;
using System.Threading;
using System.Threading.Tasks;
using DifBot.Common.Extensions;
using DifBot.Config;
using DifBot.Services;
using DifBot.Services.Loggers;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DifBot.NotificationHandlers;

public class ClientStatusHandler : INotificationHandler<ClientHeartbeatNotification>,
                                    INotificationHandler<SessionCreatedOrResumedNotification>,
                                    INotificationHandler<ClientDisconnected>
{
    // POC: Make better
    public static bool IsClientActuallyReady = false;

    private readonly BotSettingsService _botSettingsService;
    private readonly MessageRefsService _messageRefsService;
    private readonly ILogger<ClientStatusHandler> _logger;
    private readonly BotOptions _options;

    public ClientStatusHandler(
        BotSettingsService botSettingsService,
        MessageRefsService messageRefsService,
        ILogger<ClientStatusHandler> logger,
        IOptions<BotOptions> options)
    {
        _botSettingsService = botSettingsService;
        _messageRefsService = messageRefsService;
        _logger = logger;
        _options = options.Value;
    }

    public Task Handle(ClientHeartbeatNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogTrace($"Heartbeated: {notification.EventArgs.Timestamp.ToIsoDateTimeString()}");

        if (!IsClientActuallyReady)
        {
            _logger.LogDebug("Client not actually ready. Skipping heartbeat save.");
            return Task.CompletedTask;
        }

        return _botSettingsService.SetLastHeartbeat(notification.EventArgs.Timestamp);
    }

    public async Task Handle(SessionCreatedOrResumedNotification notification, CancellationToken cancellationToken)
    {
        var lastHeartbeat = await _botSettingsService.GetLastHeartbeat() ?? DateTimeOffset.UtcNow;

        var timeSinceLastHeartbeat = DateTimeOffset.UtcNow - lastHeartbeat;

        if (timeSinceLastHeartbeat.TotalMinutes > 1)
        {
            var message = $"Client ready or resumed. Last heartbeat: {lastHeartbeat.ToIsoDateTimeString()}.";
            message += $" More than {timeSinceLastHeartbeat.TotalMinutes:0.00} minutes since last heartbeat.";

            _logger.LogDebug(message);
        }

        if (_options.AutoPopulateMessagesOnReadyEnabled)
        {
            var createdCount = await _messageRefsService.PopulateMessageRefs(lastHeartbeat, notification.EventSource);

            if (createdCount > 0)
            {
                _logger.LogDebug($"Populated {createdCount} message refs");
            }
        }

        IsClientActuallyReady = true;
    }

    public Task Handle(ClientDisconnected notification, CancellationToken cancellationToken)
    {
        IsClientActuallyReady = false;

        _logger.LogInformation($"Bot disconected {notification.EventArgs.CloseMessage}");

        return Task.CompletedTask;
    }
}
