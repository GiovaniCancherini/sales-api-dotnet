using FluentValidation;

namespace SalesApi.Application.Sales.GetAllSale;

/// <summary>
/// Validator for GetAllSaleCommand
/// </summary>
public class GetAllSaleValidator : AbstractValidator<GetAllSaleQuery>
{
    /// <summary>
    /// Initializes validation rules for GetAllSaleCommand
    /// </summary>
    public GetAllSaleValidator()
    {
    }
}
