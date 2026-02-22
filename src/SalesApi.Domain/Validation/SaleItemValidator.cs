using SalesApi.Domain.Entities;
using SalesApi.Domain.Enums;
using FluentValidation;

namespace SalesApi.Domain.Validation;

/// <summary>
/// Validator for SaleItem entity.
/// Ensures pricing and quantity rules are respected.
/// </summary>
public class SaleItemValidator : AbstractValidator<SaleItem>
{
    public SaleItemValidator()
    {
        RuleFor(i => i.Product)
            .NotNull()
            .WithMessage("Product is required.");

        RuleFor(i => i.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");

        RuleFor(i => i.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than zero.");

        RuleFor(i => i.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Discount cannot be negative.");

        RuleFor(i => i)
            .Must(i => i.Discount <= (i.Quantity * i.UnitPrice))
            .WithMessage("Discount cannot exceed item subtotal.");
    }
}
