using MediatR;
using Microsoft.Extensions.Logging;
using SalesApi.Domain.Events;

namespace SalesApi.Application.Sales.Events;

public class ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger) : INotificationHandler<ItemCancelledEvent>
{
    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Item {ItemId} of sale {SaleId} cancelled at {OccurredOn}",
            notification.ItemId, notification.SaleId, notification.OccurredOn);
        return Task.CompletedTask;
    }
}
