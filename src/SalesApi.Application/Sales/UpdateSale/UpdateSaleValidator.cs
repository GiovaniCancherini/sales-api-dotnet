using FluentValidation;

namespace SalesApi.Application.Sales.UpdateSale;

/// <summary>
/// Validator for UpdateSaleCommand
/// </summary>
public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
{
    /// <summary>
    /// Initializes validation rules for UpdateSaleCommand
    /// </summary>
    public UpdateSaleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Sale ID is required");
    }
}
