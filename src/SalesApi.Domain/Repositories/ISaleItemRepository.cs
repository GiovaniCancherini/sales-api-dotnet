using SalesApi.Domain.Entities;

namespace SalesApi.Domain.Repositories;

public interface ISaleItemRepository
{
    /// <summary>
    /// Add a new saleItem in the repository
    /// </summary>
    /// <param name="saleItem">The saleItem to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created saleItem</returns>
    Task<SaleItem> AddAsync(SaleItem saleItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a saleItem by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the saleItem</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The saleItem if found, null otherwise</returns>
    Task<SaleItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all saleItems by their unique identifier a sale
    /// </summary>
    /// <param name="idSale">The unique identifier of the sale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All saleItems, null otherwise</returns>
    Task<IEnumerable<SaleItem>> GetAllByIdSaleAsync(Guid idSale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all saleItems
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All saleItems, null otherwise</returns>
    Task<IEnumerable<SaleItem>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a saleItem from the repository
    /// </summary>
    /// <param name="saleItem">The saleItem to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the saleItem was updated, false if not found</returns>
    Task<bool> UpdateAsync(SaleItem saleItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a saleItem from the repository
    /// </summary>
    /// <param name="id">The unique identifier of the saleItem to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the saleItem was deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}