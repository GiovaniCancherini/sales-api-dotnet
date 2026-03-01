using SalesApi.Domain.Entities;

namespace SalesApi.Domain.Repositories;

/// <summary>
/// Repository contract for managing persistence operations
/// of the <see cref="Sale"/> aggregate root.
/// </summary>
public interface ISaleRepository
{
    /// <summary>
    /// Creates a new sale in the repository.
    /// </summary>
    /// <param name="sale">The sale to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale</returns>
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sale by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the sale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale if found, null otherwise</returns>
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sale by its business sale number.
    /// </summary>
    Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves all sales.
    /// </summary>
    Task<List<Sale>?> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a sale from the repository.
    /// </summary>
    Task<Sale?> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a sale from the repository.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a sale with the specified sale number exists.
    /// </summary>
    Task<bool> ExistsAsync(string saleNumber, CancellationToken cancellationToken = default);
}
