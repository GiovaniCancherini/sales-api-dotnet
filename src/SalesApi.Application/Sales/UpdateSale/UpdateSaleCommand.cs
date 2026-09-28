using SalesApi.Application.Sales.CreateSale;
using MediatR;

namespace SalesApi.Application.Sales.UpdateSale;

/// <summary>
/// Command for updating an existing sale's items.
/// Customer/Branch/SaleNumber are historical facts of the sale and are never
/// editable here - Sale has no domain method to change them after creation.
/// Status changes only through the dedicated Cancel operation, not Update.
/// </summary>
public class UpdateSaleCommand : IRequest<UpdateSaleResponse>
{
    public Guid Id { get; set; }

    /// <summary>
    /// The new, complete list of items for this sale (replaces the existing ones).
    /// Reuses CreateSaleItemCommand - same flat shape, same construction rules.
    /// </summary>
    public ICollection<CreateSaleItemCommand> Items { get; set; } = new List<CreateSaleItemCommand>();
}
