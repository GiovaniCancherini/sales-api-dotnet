using SalesApi.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace SalesApi.Application.Sales.CancelSaleItem;

/// <summary>
/// Handler for processing CancelSaleItemCommand requests
/// </summary>
public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDistributedCache _cache;

    public CancelSaleItemHandler(ISaleRepository saleRepository, IDistributedCache cache)
    {
        _saleRepository = saleRepository;
        _cache = cache;
    }

    public async Task<CancelSaleItemResponse> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found");

        sale.CancelItem(request.ItemId);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        // After the commit: invalidating before would let a concurrent read re-cache the old value.
        await _cache.RemoveAsync(SaleCacheKeys.ById(request.SaleId), cancellationToken);

        return new CancelSaleItemResponse { Success = true };
    }
}
