using SalesApi.Domain.Entities;

namespace SalesApi.Domain.Repositories;

public interface ISaleItemRepository
{
    Task<SaleItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<SaleItem>> GetAllAsync();
    Task AddAsync(SaleItem saleItem);
    Task UpdateAsync(SaleItem saleItem);
    Task DeleteAsync(Guid id);
}