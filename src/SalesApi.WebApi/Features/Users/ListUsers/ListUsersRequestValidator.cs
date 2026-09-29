using FluentValidation;

namespace SalesApi.WebApi.Features.Users.ListUsers;

/// <summary>
/// Validator for ListUsersRequest
/// </summary>
public class ListUsersRequestValidator : AbstractValidator<ListUsersRequest>
{
    /// <summary>
    /// Initializes validation rules for ListUsersRequest
    /// </summary>
    public ListUsersRequestValidator()
    {
        // No required fields for this request - Page/Size/Order are all
        // optional with sane defaults.
    }
}
