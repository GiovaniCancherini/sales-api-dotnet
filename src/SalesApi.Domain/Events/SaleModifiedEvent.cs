using SalesApi.Domain.Common;

namespace SalesApi.Domain.Events;

public sealed record SaleModifiedEvent(Guid SaleId) : DomainEvent;