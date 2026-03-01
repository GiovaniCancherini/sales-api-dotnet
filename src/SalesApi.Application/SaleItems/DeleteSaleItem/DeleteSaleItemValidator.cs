using FluentValidation;

namespace SalesApi.Application.SaleItems.DeleteSaleItem;

/// <summary>
/// Validator for DeleteSaleItemCommand
/// </summary>
public class DeleteSaleItemValidator : AbstractValidator<DeleteSaleItemCommand>
{
    /// <summary>
    /// Initializes validation rules for DeleteSaleItemCommand
    /// </summary>
    public DeleteSaleItemValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("SaleItem ID is required");
    }
}
