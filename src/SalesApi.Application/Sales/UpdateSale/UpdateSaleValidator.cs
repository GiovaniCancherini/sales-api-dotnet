using FluentValidation;

namespace SalesApi.Application.Sales.UpdateSale;

/// <summary>
/// Validator for UpdateSaleCommand
/// </summary>
public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Sale ID is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("A sale must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product ID is required.")
                .Must(id => Guid.TryParse(id, out _)).WithMessage("Product ID must be a valid GUID.");

            item.RuleFor(i => i.ProductName).NotEmpty();
            item.RuleFor(i => i.ProductSku).NotEmpty();
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .LessThanOrEqualTo(20).WithMessage("Cannot sell more than 20 identical items.");
            item.RuleFor(i => i.UnitPrice).GreaterThan(0);
        });
    }
}
