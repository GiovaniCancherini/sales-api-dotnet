using MediatR;
using Microsoft.Extensions.Logging;
using SalesApi.Domain.Events;

namespace SalesApi.Application.Sales.Events;

public class SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger) : INotificationHandler<SaleCancelledEvent>
{
    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sale {SaleId} cancelled at {OccurredOn}", notification.SaleId, notification.OccurredOn);
        return Task.CompletedTask;
    }
}
