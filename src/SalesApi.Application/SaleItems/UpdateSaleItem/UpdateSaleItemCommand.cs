using MediatR;

namespace SalesApi.Application.SaleItems.UpdateSaleItem;

/// <summary>
/// Command for updating an existing sale item.
/// </summary>
/// <remarks>
/// This command is used to update the quantity or unit price of an item within a sale.
/// </remarks>
public record UpdateSaleItemCommand : IRequest<UpdateSaleItemResponse>
{
    /// <summary>
    /// The unique identifier of the sale item to update.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The ID of the parent sale to which the item belongs.
    /// </summary>
    public Guid SaleId { get; init; }

    /// <summary>
    /// The new quantity for the sale item.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// The new unit price for the sale item.
    /// </summary>
    public decimal UnitPrice { get; init; }

}