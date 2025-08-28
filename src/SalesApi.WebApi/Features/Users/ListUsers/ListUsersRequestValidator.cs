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
        // Caso não tenha filtros obrigatórios, pode deixar vazio por enquanto
        // Exemplo futuro: RuleFor(x => x.Name).MaximumLength(100);
    }
}
