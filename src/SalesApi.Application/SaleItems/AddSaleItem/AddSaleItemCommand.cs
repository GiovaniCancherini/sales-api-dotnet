using SalesApi.Common.Validation;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Enums;
using MediatR;

namespace SalesApi.Application.SaleItems.AddSaleItem;

/// <summary>
/// Command for creating a new saleItem.
/// </summary>
/// <remarks>
/// This command is used for an item to be added to a sale.
/// It implements <see cref="IRequest{TResponse}"/> to initiate the request 
/// that returns a <see cref="AddSaleItemResult"/>.
/// 
/// The data provided in this command is validated using the 
/// <see cref="AddSaleItemCommandValidator"/> which extends 
/// <see cref="AbstractValidator{T}"/> to ensure that the fields are correctly 
/// populated and follow the required rules.
/// </remarks>
public class AddSaleItemCommand : IRequest<AddSaleItemResult>
{
    /// <summary>
    /// Gets or sets the ID of the product.
    /// </summary>
    public string ProductId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of the product.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Gets or sets the unit price of the product.
    /// </summary>
    public decimal UnitPrice { get; init; }

    public ValidationResultDetail Validate()
    {
        var validator = new AddSaleItemCommandValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}