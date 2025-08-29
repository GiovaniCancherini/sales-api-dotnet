using FluentValidation;

namespace SalesApi.Application.SaleItems.GetByIdSale;

/// <summary>
/// Validator for GetBySaleItemIdCommand
/// </summary>
public class GetByIdSaleValidator : AbstractValidator<GetByIdSaleQuery>
{
    /// <summary>
    /// Initializes validation rules for GetBySaleItemIdCommand
    /// </summary>
    public GetByIdSaleValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty()
            .WithMessage("Sale ID is required");
    }
}
