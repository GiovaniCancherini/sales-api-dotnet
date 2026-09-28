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

    public async Task<List<Sale>?> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sale?> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        // No explicit Sales.Update() call: every caller today loads the Sale via
        // GetByIdAsync first (same DbContext), so it's already tracked.
        // DbSet.Update() marks the WHOLE reachable graph as Modified,
        // including brand-new SaleItems that should be Added (insert).
        //
        // That alone isn't enough though: SaleItem's constructor assigns
        // Id = Guid.NewGuid() itself, so a new item already has a non-default
        // key by the time EF's change detection discovers it in the Items
        // collection. EF then assumes it might already exist in the DB and
        // marks it Modified instead of Added, producing an UPDATE that
        // matches 0 rows (DbUpdateConcurrencyException).
        //
        // Checking Entry(item).State == Detached to spot new items is NOT
        // reliable: it only works when the whole collection was replaced
        // (Clear + AddRange, as ReplaceItems does). When an item is appended
        // to an otherwise-unchanged tracked collection (AddItem's _items.Add),
        // EF's ambiguous key heuristic kicks in immediately and the new item
        // is already Modified by the time this method sees it - Detached
        // never happens.
        //
        // Checking Entry(item).State == Unchanged doesn't work either: a
        // genuinely pre-existing item that WAS legitimately modified (e.g.
        // CancelItem flipping IsCancelled) is no longer Unchanged, so it'd be
        // wrongly treated as new and re-Added on top of its own row.
        //
        // The only unambiguous signal is what's actually already in the
        // database - so ask it directly rather than trust EF's in-memory
        // guess about a client-generated key it never assigned itself.
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
                // SaleItems.Add(item) (rather than poking .State by hand)
                // also correctly cascades Added to Product, its table-split
                // OwnsOne - manual state assignment left ProductId NULL.
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