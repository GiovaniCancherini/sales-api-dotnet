using FluentValidation;

namespace SalesApi.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Validator for CreateSaleRequest that defines validation rules for sale creation.
/// </summary>
public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(sale => sale.SaleNumber).NotEmpty();
        RuleFor(sale => sale.Date).LessThanOrEqualTo(DateTime.UtcNow);

        RuleFor(sale => sale.CustomerId)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _)).WithMessage("CustomerId must be a valid GUID.");
        RuleFor(sale => sale.CustomerName).NotEmpty().Length(3, 100);

        RuleFor(sale => sale.BranchId)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _)).WithMessage("BranchId must be a valid GUID.");
        RuleFor(sale => sale.BranchName).NotEmpty();
        RuleFor(sale => sale.BranchCode).NotEmpty();

        RuleFor(sale => sale.Items).NotEmpty().WithMessage("A sale must contain at least one item.");

        RuleForEach(sale => sale.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty()
                .Must(id => Guid.TryParse(id, out _)).WithMessage("ProductId must be a valid GUID.");
            item.RuleFor(i => i.ProductName).NotEmpty();
            item.RuleFor(i => i.ProductSku).NotEmpty();
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .LessThanOrEqualTo(20).WithMessage("Cannot sell more than 20 identical items.");
            item.RuleFor(i => i.UnitPrice).GreaterThan(0);
        });
    }
}
