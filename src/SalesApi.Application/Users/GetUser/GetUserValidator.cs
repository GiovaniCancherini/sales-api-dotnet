using FluentValidation;

namespace SalesApi.Application.Users.GetUser;

/// <summary>
/// Validator for GetUserCommand
/// </summary>
public class GetAllUserValidator : AbstractValidator<GetUserQuery>
{
    /// <summary>
    /// Initializes validation rules for GetUserCommand
    /// </summary>
    public GetAllUserValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
