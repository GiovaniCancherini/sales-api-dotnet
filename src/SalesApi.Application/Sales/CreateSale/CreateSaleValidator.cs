using SalesApi.Application.SaleItems.AddSaleItem;
using SalesApi.Domain.Enums;
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
            .NotEmpty().WithMessage("O número da venda não pode estar vazio.");

        RuleFor(sale => sale.Date)
            .LessThanOrEqualTo(DateTime.Now).WithMessage("A data da venda não pode ser futura.");

        RuleFor(sale => sale.CustomerId)
            .NotEmpty().WithMessage("O ID do cliente é obrigatório.");

        RuleFor(sale => sale.CustomerName)
            .NotEmpty().WithMessage("O nome do cliente é obrigatório.")
            .Length(3, 100).WithMessage("O nome do cliente deve ter entre 3 e 100 caracteres.");

        RuleFor(sale => sale.BranchId)
            .NotEmpty().WithMessage("O ID da filial é obrigatório.");

        RuleFor(sale => sale.Status)
            .NotEqual(SaleStatus.Unknown).WithMessage("O status da venda não pode ser 'Unknown'.");

        RuleFor(sale => sale.TotalAmount)
            .GreaterThan(0).WithMessage("O valor total da venda deve ser maior que zero.");

        // Regra para validar a lista de itens e cada item individualmente
        RuleFor(sale => sale.Items)
            .NotEmpty().WithMessage("A venda deve conter pelo menos um item.");
    }
}