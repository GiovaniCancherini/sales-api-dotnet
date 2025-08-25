using MediatR;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Enums;

namespace SalesApi.Application.Sales.UpdateSale;

/// <summary>
/// Command for updating an existing sale.
/// </summary>
public record UpdateSaleCommand : IRequest<UpdateSaleResponse>
{
    /// <summary>
    /// The unique identifier of the sale to update.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The updated ID of the customer for the sale.
    /// </summary>
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The updated name of the customer for the sale.
    /// </summary>
    public string CustomerName { get; init; } = string.Empty;

    /// <summary>
    /// The updated ID of the branch where the sale was made.
    /// </summary>
    public string BranchId { get; init; } = string.Empty;

    /// <summary>
    /// The updated name of the branch where the sale was made.
    /// </summary>
    public string BranchName { get; init; } = string.Empty;

    /// <summary>
    /// The new status of the sale.
    /// </summary>
    public SaleStatus Status { get; init; }

    /// <summary>
    /// The collection of items to update in the sale.
    /// </summary>
    public ICollection<SaleItem> Items { get; init; } = new List<SaleItem>();

    /// <summary>
    /// Initializes a new instance of the UpdateSaleCommand.
    /// </summary>
    public UpdateSaleCommand(Guid id, string customerId, string customerName, string branchId, string branchName, SaleStatus status, ICollection<SaleItem> items)
    {
        Id = id;
        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;
        Status = status;
        Items = items;
    }
}