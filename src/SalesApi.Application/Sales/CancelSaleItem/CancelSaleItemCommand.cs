using MediatR;

namespace SalesApi.Application.Sales.CancelSaleItem;

/// <summary>
/// Command for cancelling one item of an existing sale, through the Sale
/// aggregate (Sale.CancelItem) - SaleItem has no life outside its aggregate root.
/// </summary>
public class CancelSaleItemCommand : IRequest<CancelSaleItemResponse>
{
    public Guid SaleId { get; set; }
    public Guid ItemId { get; set; }

    public CancelSaleItemCommand(Guid saleId, Guid itemId)
    {
        SaleId = saleId;
        ItemId = itemId;
    }
}
