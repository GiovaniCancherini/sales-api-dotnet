using SalesApi.Domain.Entities;

namespace SalesApi.Domain.Repositories;

public interface ISaleRepository
{
    Task<Sale?> GetByIdAsync(Guid id);
    Task<IEnumerable<Sale>> GetAllAsync();
    Task AddAsync(Sale sale);
    Task UpdateAsync(Sale sale);
    Task DeleteAsync(Guid id);
}
