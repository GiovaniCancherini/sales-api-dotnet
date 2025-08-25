using FluentValidation;

namespace SalesApi.Application.Sales.GetBySaleId;

/// <summary>
/// Validator for GetBySaleIdCommand
/// </summary>
public class GetBySaleIdValidator : AbstractValidator<GetBySaleIdQuery>
{
    /// <summary>
    /// Initializes validation rules for GetBySaleIdCommand
    /// </summary>
    public GetBySaleIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Sale ID is required");
    }
}
