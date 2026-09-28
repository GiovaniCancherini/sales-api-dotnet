namespace SalesApi.WebApi.Features.Sales.CreateSale;

/// <summary>
/// API response model for the CreateSale operation.
/// </summary>
public class CreateSaleResponse
{
    /// <summary>
    /// The unique identifier of the newly created sale.
    /// </summary>
    public Guid Id { get; set; }
}
