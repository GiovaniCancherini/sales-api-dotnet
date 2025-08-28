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
        
    }
}
