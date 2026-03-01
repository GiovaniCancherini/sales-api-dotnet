using SalesApi.Domain.Common;

namespace SalesApi.Domain.Events;

public sealed record SaleCancelledEvent(Guid SaleId) : DomainEvent;