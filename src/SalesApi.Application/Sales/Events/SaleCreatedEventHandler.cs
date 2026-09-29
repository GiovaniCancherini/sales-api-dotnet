using MediatR;
using Microsoft.Extensions.Logging;
using SalesApi.Domain.Events;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.Sales.Events;

public class SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger, ISaleEventStore eventStore) : INotificationHandler<SaleCreatedEvent>
{
    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sale {SaleId} created at {OccurredOn}", notification.SaleId, notification.OccurredOn);
        return eventStore.AppendAsync(notification, notification.SaleId, cancellationToken: cancellationToken);
    }
}
