using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;
using SalesApi.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace SalesApi.Application.Sales.UpdateSale;

/// <summary>
/// Handler for processing UpdateSaleCommand requests. Loads the Sale, builds
/// SaleItem entities directly from the command's flat data (no AutoMapper -
/// SaleItem only exposes a constructor + private setters), then replaces the
/// item list via Sale.ReplaceItems.
/// </summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDistributedCache _cache;

    public UpdateSaleHandler(ISaleRepository saleRepository, IDistributedCache cache)
    {
        _saleRepository = saleRepository;
        _cache = cache;
    }

    public async Task<UpdateSaleResponse> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        var newItems = request.Items.Select(item =>
        {
            var product = new ExternalProduct(Guid.Parse(item.ProductId), item.ProductName, item.ProductSku);
            return new SaleItem(product, item.Quantity, item.UnitPrice);
        }).ToList();

        sale.ReplaceItems(newItems);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        // After the commit: invalidating before would let a concurrent read re-cache the old value.
        await _cache.RemoveAsync(SaleCacheKeys.ById(request.Id), cancellationToken);

        return new UpdateSaleResponse { Success = true };
    }
}
