using AutoMapper;
using MediatR;
using FluentValidation;
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
    /// <param name="validator">The validator for GetAllUserCommand</param>
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
    /// <returns>The user details if found</returns>
    public async Task<List<GetAllUserResult>> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetAllUserValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var users = await _userRepository.GetAllAsync(cancellationToken);
        if (users == null || !users.Any())
            throw new KeyNotFoundException("No Users found");

        return _mapper.Map<List<GetAllUserResult>>(users);
    }
}
