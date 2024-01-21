﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace DifBot.Infrastructure;

public class NotificationPublisher
{
    private readonly NotificationMediator _mediator;

    public NotificationPublisher(IServiceProvider serviceProvider)
    {
        _mediator = new NotificationMediator(serviceProvider, SyncContinueOnException);
    }

    public Task Publish(INotification notification, CancellationToken cancellationToken) => _mediator.Publish(notification, cancellationToken);

    /// <summary>
    /// Sauce: https://github.com/jbogard/MediatR/blob/master/samples/MediatR.Examples.PublishStrategies/Publisher.cs.
    /// </summary>
    private async Task SyncContinueOnException(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken cancellationToken)
    {
        var exceptions = new List<Exception>();

        foreach (var handler in handlers)
        {
            try
            {
                await handler.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                exceptions.AddRange(ex.Flatten().InnerExceptions);
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }
    }
}
