using SalesApi.Domain.Enums;
using FluentValidation;

namespace SalesApi.Application.SaleItems.AddSaleItem;

/// <summary>
/// Validator for AddSaleItemCommand that defines validation rules for user creation command.
/// </summary>
public class AddSaleItemCommandValidator : AbstractValidator<AddSaleItemCommand>
{
    /// <summary>
    /// Initializes a new instance of the AddSaleItemCommandValidator with defined validation rules.
    /// </summary>
    /// <remarks>
    /// Validation rules include:
    /// - ProductId: Required.
    /// - ProductName: Required, and must be between 3 and 100 characters.
    /// - Quantity: Must be greater than 0 and less than or equal to 20.
    /// - UnitPrice: Must be greater than 0.
    /// </remarks>
    public AddSaleItemCommandValidator()
    {
        RuleFor(item => item.ProductId)
           .NotEmpty().WithMessage("O ID do produto é obrigatório.");

        RuleFor(item => item.ProductName)
            .NotEmpty().WithMessage("O nome do produto é obrigatório.")
            .Length(3, 100).WithMessage("O nome do produto deve ter entre 3 e 100 caracteres.");

        RuleFor(item => item.Quantity)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.")
            .LessThanOrEqualTo(20).WithMessage("Não é possível vender mais de 20 itens idênticos por vez.");

        RuleFor(item => item.UnitPrice)
            .GreaterThan(0).WithMessage("O preço unitário deve ser maior que zero.");
    }
}