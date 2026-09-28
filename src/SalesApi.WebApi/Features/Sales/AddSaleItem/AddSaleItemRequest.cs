using SalesApi.WebApi.Features.Sales.CreateSale;

namespace SalesApi.WebApi.Features.Sales.AddSaleItem;

/// <summary>
/// Adds one item to an existing sale. Reuses CreateSaleItemRequest - same
/// shape, and AutoMapper already knows how to map it to CreateSaleItemCommand.
/// </summary>
public class AddSaleItemRequest : CreateSaleItemRequest
{
}
