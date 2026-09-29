using MediatR;
using Microsoft.Extensions.Logging;
using SalesApi.Domain.Events;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.Sales.Events;

public class SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger, ISaleEventStore eventStore) : INotificationHandler<SaleCancelledEvent>
{
    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sale {SaleId} cancelled at {OccurredOn}", notification.SaleId, notification.OccurredOn);
        return eventStore.AppendAsync(notification, notification.SaleId, cancellationToken: cancellationToken);
    }
}
