using SalesApi.Domain.Enums;

namespace SalesApi.Application.SaleItems.GetBySaleItemId;

/// <summary>
/// Response model for GetBySaleItemId operation
/// </summary>
public class GetBySaleItemIdResult
{
    /// <summary>
    /// The unique identifier of the sale
    /// </summary>
    public Guid Id { get; set; }

}
