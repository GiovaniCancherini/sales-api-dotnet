using SalesApi.Domain.Common;

namespace SalesApi.Domain.Events;

public sealed record SaleCreatedEvent(Guid SaleId) : DomainEvent;