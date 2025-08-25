using FluentValidation;

namespace SalesApi.Application.SaleItems.GetBySaleItemId;

/// <summary>
/// Validator for GetBySaleItemIdCommand
/// </summary>
public class GetBySaleItemItemIdValidator : AbstractValidator<GetBySaleItemIdQuery>
{
    /// <summary>
    /// Initializes validation rules for GetBySaleItemIdCommand
    /// </summary>
    public GetBySaleItemItemIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Sale Items ID is required");
    }
}
