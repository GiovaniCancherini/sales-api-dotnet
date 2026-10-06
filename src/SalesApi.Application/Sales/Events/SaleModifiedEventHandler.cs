using MediatR;
using Microsoft.Extensions.Logging;
using SalesApi.Domain.Events;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.Sales.Events;

public class SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger, ISaleEventStore eventStore) : INotificationHandler<SaleModifiedEvent>
{
    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sale {SaleId} modified at {OccurredOn}", notification.SaleId, notification.OccurredOn);
        return eventStore.AppendAsync(notification, notification.SaleId, cancellationToken: cancellationToken);
    }
}
