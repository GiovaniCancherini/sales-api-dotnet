using SalesApi.Domain.Common;

namespace SalesApi.Domain.Repositories;

/// <summary>
/// Append-only log of the Sale aggregate's domain events
/// (SaleCreated, SaleModified, SaleCancelled, ItemCancelled).
/// </summary>
public interface ISaleEventStore
{
    /// <summary>
    /// Appends one sale event to the log.
    /// </summary>
    Task AppendAsync(DomainEvent saleEvent, Guid saleId, Guid? itemId = null, CancellationToken cancellationToken = default);
}
