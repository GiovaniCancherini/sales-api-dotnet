using FluentValidation;

namespace SalesApi.Application.Users.GetAllUser;

/// <summary>
/// Validator for GetAllUserCommand
/// </summary>
public class GetAllUserValidator : AbstractValidator<GetAllUserQuery>
{
    /// <summary>
    /// Initializes validation rules for GetAllUserCommand
    /// </summary>
    public GetAllUserValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("_page must be at least 1.");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100).WithMessage("_size must be between 1 and 100.");
    }
}
