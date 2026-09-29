using AutoMapper;
using MediatR;
using FluentValidation;
using SalesApi.Application.Common;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.Users.GetAllUser;

/// <summary>
/// Handler for processing GetAllUserQuery requests: ordering and pagination
/// run in the database.
/// </summary>
public class GetAllUserHandler : IRequestHandler<GetAllUserQuery, (List<GetAllUserResult> Items, int TotalCount)>
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
    /// Handles the GetAllUserQuery request
    /// </summary>
    /// <param name="request">The GetAllUser query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The requested page of users and the total user count</returns>
    public async Task<(List<GetAllUserResult> Items, int TotalCount)> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetAllUserValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var (users, totalCount) = await _userRepository.ListAsync(
            query => ApplyOrder(query, request.Order),
            request.Page, request.Size, cancellationToken);

        return (_mapper.Map<List<GetAllUserResult>>(users), totalCount);
    }

    private static IQueryable<User> ApplyOrder(IQueryable<User> query, string? order)
    {
        IOrderedQueryable<User>? ordered = null;

        foreach (var (field, descending) in QueryOrdering.Parse(order))
        {
            ordered = field switch
            {
                "email" => QueryOrdering.Then(query, ordered, u => u.Email, descending),
                "role" => QueryOrdering.Then(query, ordered, u => u.Role, descending),
                "status" => QueryOrdering.Then(query, ordered, u => u.Status, descending),
                _ => QueryOrdering.Then(query, ordered, u => u.Username, descending)
            };
        }

        // Id breaks ties (Username isn't unique) so pages don't shuffle rows.
        return (ordered ?? query.OrderBy(u => u.Username)).ThenBy(u => u.Id);
    }
}
