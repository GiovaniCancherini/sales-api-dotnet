using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;
using SalesApi.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace SalesApi.Application.Sales.AddSaleItem;

/// <summary>
/// Handler for processing AddSaleItemCommand requests. Loads the Sale, builds
/// the new SaleItem from the command's flat data (no AutoMapper - SaleItem only
/// exposes a constructor + private setters), then adds it via Sale.AddItem.
/// </summary>
public class AddSaleItemHandler : IRequestHandler<AddSaleItemCommand, AddSaleItemResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDistributedCache _cache;

    public AddSaleItemHandler(ISaleRepository saleRepository, IDistributedCache cache)
    {
        _saleRepository = saleRepository;
        _cache = cache;
    }

    public async Task<AddSaleItemResponse> Handle(AddSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found");

        var product = new ExternalProduct(Guid.Parse(request.Item.ProductId), request.Item.ProductName, request.Item.ProductSku);
        var item = new SaleItem(product, request.Item.Quantity, request.Item.UnitPrice);

        sale.AddItem(item);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        // After the commit: invalidating before would let a concurrent read re-cache the old value.
        await _cache.RemoveAsync(SaleCacheKeys.ById(request.SaleId), cancellationToken);

        return new AddSaleItemResponse { Success = true };
    }
}
