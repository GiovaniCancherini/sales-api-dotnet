using MediatR;
using Microsoft.Extensions.Logging;
using SalesApi.Domain.Events;

namespace SalesApi.Application.Sales.Events;

public class SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger) : INotificationHandler<SaleModifiedEvent>
{
    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sale {SaleId} modified at {OccurredOn}", notification.SaleId, notification.OccurredOn);
        return Task.CompletedTask;
    }
}
