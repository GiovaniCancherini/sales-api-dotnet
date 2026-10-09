using SalesApi.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace SalesApi.Application.Sales.CancelSale;

/// <summary>
/// Handler for processing CancelSaleCommand requests
/// </summary>
public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDistributedCache _cache;

    public CancelSaleHandler(ISaleRepository saleRepository, IDistributedCache cache)
    {
        _saleRepository = saleRepository;
        _cache = cache;
    }

    public async Task<CancelSaleResponse> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        sale.Cancel();

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        // After the commit: invalidating before would let a concurrent read re-cache the old value.
        await _cache.RemoveAsync(SaleCacheKeys.ById(request.Id), cancellationToken);

        return new CancelSaleResponse { Success = true };
    }
}
