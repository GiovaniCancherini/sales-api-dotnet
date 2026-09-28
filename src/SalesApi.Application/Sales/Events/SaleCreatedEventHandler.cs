using MediatR;
using Microsoft.Extensions.Logging;
using SalesApi.Domain.Events;

namespace SalesApi.Application.Sales.Events;

public class SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger) : INotificationHandler<SaleCreatedEvent>
{
    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sale {SaleId} created at {OccurredOn}", notification.SaleId, notification.OccurredOn);
        return Task.CompletedTask;
    }
}
