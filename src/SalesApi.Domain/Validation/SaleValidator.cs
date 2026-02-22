using SalesApi.Domain.Entities;
using SalesApi.Domain.Enums;
using FluentValidation;

namespace SalesApi.Domain.Validation;

/// <summary>
/// Validator for the Sale aggregate.
/// Ensures business invariants are respected before persistence.
/// </summary>
public class SaleValidator : AbstractValidator<Sale>
{
    public SaleValidator()
    {
        RuleFor(s => s.SaleNumber)
            .NotEmpty()
            .WithMessage("Sale number is required.");

        RuleFor(s => s.Customer)
            .NotNull()
            .WithMessage("Customer is required.");

        RuleFor(s => s.Branch)
            .NotNull()
            .WithMessage("Branch is required.");

        RuleFor(s => s.Items)
            .NotNull()
            .Must(items => items.Any())
            .WithMessage("A sale must contain at least one item.");

        RuleFor(s => s.TotalAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total amount cannot be negative.");
    }
}
