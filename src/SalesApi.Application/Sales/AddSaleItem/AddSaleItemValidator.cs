using FluentValidation;

namespace SalesApi.Application.Sales.AddSaleItem;

/// <summary>
/// Validator for AddSaleItemCommand
/// </summary>
public class AddSaleItemValidator : AbstractValidator<AddSaleItemCommand>
{
    public AddSaleItemValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty().WithMessage("Sale ID is required");

        RuleFor(x => x.Item).NotNull();

        When(x => x.Item != null, () =>
        {
            RuleFor(x => x.Item.ProductId)
                .NotEmpty().WithMessage("Product ID is required.")
                .Must(id => Guid.TryParse(id, out _)).WithMessage("Product ID must be a valid GUID.");

            RuleFor(x => x.Item.ProductName).NotEmpty();
            RuleFor(x => x.Item.ProductSku).NotEmpty();
            RuleFor(x => x.Item.Quantity)
                .GreaterThan(0)
                .LessThanOrEqualTo(20).WithMessage("Cannot sell more than 20 identical items.");
            RuleFor(x => x.Item.UnitPrice).GreaterThan(0);
        });
    }
}
