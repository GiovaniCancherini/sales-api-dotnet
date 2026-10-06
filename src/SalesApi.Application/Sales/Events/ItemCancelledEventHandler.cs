using MediatR;
using Microsoft.Extensions.Logging;
using SalesApi.Domain.Events;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.Sales.Events;

public class ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger, ISaleEventStore eventStore) : INotificationHandler<ItemCancelledEvent>
{
    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Item {ItemId} of sale {SaleId} cancelled at {OccurredOn}",
            notification.ItemId, notification.SaleId, notification.OccurredOn);
        return eventStore.AppendAsync(notification, notification.SaleId, notification.ItemId, cancellationToken);
    }
}
