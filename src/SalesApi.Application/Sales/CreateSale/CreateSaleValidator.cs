using FluentValidation;

namespace SalesApi.Application.Sales.CreateSale;

/// <summary>
/// Validator for CreateSaleCommand that defines validation rules for user creation command.
/// </summary>
public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    /// <summary>
    /// Initializes a new instance of the CreateSaleCommandValidator with defined validation rules.
    /// </summary>
    /// <remarks>
    /// Validation rules include:
    /// - SaleNumber: Must not be empty.
    /// - Date: Must not be in the future.
    /// - CustomerId: Required.
    /// - CustomerName: Required, and must be between 3 and 100 characters.
    /// - BranchId: Required.
    /// - Status: Must not be set to Unknown.
    /// - TotalAmount: Must be greater than zero.
    /// - Items: Must not be empty, and each item must be valid (using SaleItemCommandValidator).
    /// </remarks>
    public CreateSaleCommandValidator()
    {
        RuleFor(sale => sale.SaleNumber)
            .NotEmpty().WithMessage("Sale number must not be empty.");

        RuleFor(sale => sale.Date)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Sale date cannot be in the future.");

        RuleFor(sale => sale.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Customer ID must be a valid GUID.");

        RuleFor(sale => sale.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .Length(3, 100).WithMessage("Customer name must be between 3 and 100 characters.");

        RuleFor(sale => sale.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Branch ID must be a valid GUID.");

        RuleFor(sale => sale.BranchName)
            .NotEmpty().WithMessage("Branch name is required.");

        RuleFor(sale => sale.BranchCode)
            .NotEmpty().WithMessage("Branch code is required.");

        // Rule to validate the item list and each item individually
        RuleFor(sale => sale.Items)
            .NotEmpty().WithMessage("A sale must contain at least one item.");

        RuleForEach(sale => sale.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product ID is required.")
                .Must(id => Guid.TryParse(id, out _)).WithMessage("Product ID must be a valid GUID.");

            item.RuleFor(i => i.ProductName)
                .NotEmpty().WithMessage("Product name is required.");

            item.RuleFor(i => i.ProductSku)
                .NotEmpty().WithMessage("Product SKU is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
                .LessThanOrEqualTo(20).WithMessage("Cannot sell more than 20 identical items.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than zero.");
        });
    }
}