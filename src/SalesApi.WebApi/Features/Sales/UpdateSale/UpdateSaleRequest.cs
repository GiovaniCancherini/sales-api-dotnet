using SalesApi.WebApi.Features.Sales.CreateSale;

namespace SalesApi.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// Replaces the full item list of an existing sale. Reuses CreateSaleItemRequest -
/// same shape, and AutoMapper already knows how to map it to CreateSaleItemCommand.
/// </summary>
public class UpdateSaleRequest
{
    public ICollection<CreateSaleItemRequest> Items { get; set; } = new List<CreateSaleItemRequest>();
}
