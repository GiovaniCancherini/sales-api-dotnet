using SalesApi.Domain.Common;

namespace SalesApi.Domain.Events;

public sealed record ItemCancelledEvent(Guid SaleId, Guid ItemId) : DomainEvent;