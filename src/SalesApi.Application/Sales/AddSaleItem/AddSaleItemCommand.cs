using SalesApi.Application.Sales.CreateSale;
using MediatR;

namespace SalesApi.Application.Sales.AddSaleItem;

/// <summary>
/// Command for adding one item to an existing sale, through the Sale
/// aggregate (Sale.AddItem) - SaleItem has no life outside its aggregate root.
/// Reuses CreateSaleItemCommand - same flat shape, same construction rules.
/// </summary>
public class AddSaleItemCommand : IRequest<AddSaleItemResponse>
{
    public Guid SaleId { get; set; }
    public CreateSaleItemCommand Item { get; set; } = new();
}
