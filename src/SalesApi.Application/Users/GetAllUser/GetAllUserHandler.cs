using AutoMapper;
using MediatR;
using FluentValidation;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.Users.GetAllUser;

/// <summary>
/// Handler for processing GetAllUserCommand requests
/// </summary>
public class GetAllUserHandler : IRequestHandler<GetAllUserQuery, List<GetAllUserResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of GetAllUserHandler
    /// </summary>
    /// <param name="userRepository">The user repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    public GetAllUserHandler(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the GetAllUserCommand request
    /// </summary>
    /// <param name="request">The GetAllUser command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user details, ordered per request.Order</returns>
    public async Task<List<GetAllUserResult>> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetAllUserValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // An empty result is a normal, valid response for a list endpoint -
        // it used to throw KeyNotFoundException (404) here, which is wrong:
        // a fresh database with zero users should return 200 with an empty list.
        var users = await _userRepository.GetAllAsync(cancellationToken) ?? new List<User>();

        return _mapper.Map<List<GetAllUserResult>>(ApplyOrder(users, request.Order));
    }

    /// <summary>
    /// Parses "field desc, field2" - same convention as GetAllSaleHandler's
    /// ApplyOrder, matching the API's _order example.
    /// </summary>
    private static IEnumerable<User> ApplyOrder(IEnumerable<User> source, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return source.OrderBy(u => u.Username);

        IOrderedEnumerable<User>? ordered = null;

        foreach (var clause in order.Trim('"').Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = clause.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = parts[0];
            var descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            Func<User, object> keySelector = field.ToLowerInvariant() switch
            {
                "name" => u => u.Username,
                "email" => u => u.Email,
                "role" => u => u.Role.ToString(),
                "status" => u => u.Status.ToString(),
                _ => u => u.Username
            };

            ordered = ordered is null
                ? (descending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector))
                : (descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector));
        }

        return ordered ?? source;
    }
}
