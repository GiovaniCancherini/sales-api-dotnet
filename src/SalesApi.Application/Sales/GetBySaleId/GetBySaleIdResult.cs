using SalesApi.Domain.Enums;

namespace SalesApi.Application.Sales.GetBySaleId;

/// <summary>
/// Response model for GetBySaleId operation
/// </summary>
public class GetBySaleIdResult
{
    /// <summary>
    /// The unique identifier of the sale
    /// </summary>
    public Guid Id { get; set; }

}
