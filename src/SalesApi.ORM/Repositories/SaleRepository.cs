using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SalesApi.ORM.Repositories;

/// <summary>
/// Implementation of ISaleRepository using Entity Framework Core.
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    /// <summary>
    /// Initializes a new instance of SaleRepository
    /// </summary>
    /// <param name="context">The database context</param>
    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new sale in the database
    /// </summary>
    /// <param name="sale">The sale to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale</returns>
    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    public async Task<(List<Sale> Items, int TotalCount)> ListAsync(
        Func<IQueryable<Sale>, IQueryable<Sale>> shape, int page, int size, CancellationToken cancellationToken = default)
    {
        // No Include(Items): the list response only exposes sale-level columns.
        var query = shape(_context.Sales.AsNoTracking());
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<Sale?> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        // SaleItem generates its own Id (Guid.NewGuid()) before being tracked, so
        // EF's Added-vs-Modified/Unchanged guess for it is unreliable in either
        // direction. Query what's actually persisted instead of trusting that guess.
        var persistedIds = (await _context.Sales
            .Where(s => s.Id == sale.Id)
            .SelectMany(s => s.Items)
            .Select(i => i.Id)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        foreach (var item in sale.Items)
        {
            if (!persistedIds.Contains(item.Id))
            {
                // Add(item), not a manual EntityState assignment: this also
                // cascades Added to the owned Product (table-split OwnsOne).
                _context.SaleItems.Add(item);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale == null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .AnyAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }
}