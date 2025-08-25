using FluentValidation;

namespace SalesApi.Application.SaleItems.GetByIdSale;

/// <summary>
/// Validator for GetBySaleItemIdCommand
/// </summary>
public class GetByIdSaleValidator : AbstractValidator<GetByIdSaleIdQuery>
{
    /// <summary>
    /// Initializes validation rules for GetBySaleItemIdCommand
    /// </summary>
    public GetByIdSaleValidator()
    {
        RuleFor(x => x.IdSale)
            .NotEmpty()
            .WithMessage("Sale ID is required");
    }
}
