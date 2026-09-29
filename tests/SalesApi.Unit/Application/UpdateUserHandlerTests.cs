using SalesApi.Application;
using SalesApi.Application.Users.UpdateUser;
using SalesApi.Common.Security;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Enums;
using SalesApi.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace SalesApi.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserHandler"/> class.
/// Uses the real AutoMapper configuration: the behavior under test is the
/// command being mapped onto the already-loaded user, which a mocked IMapper
/// would silently skip.
/// </summary>
public class UpdateUserHandlerTests
{
    private const string PlainPassword = "Test@12345";
    private const string HashedPassword = "h@shedPassw0rd";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _passwordHasher.HashPassword(PlainPassword).Returns(HashedPassword);

        var mapper = new MapperConfiguration(cfg => cfg.AddMaps(typeof(ApplicationLayer).Assembly), NullLoggerFactory.Instance)
            .CreateMapper();
        _handler = new UpdateUserHandler(_userRepository, mapper, _passwordHasher);
    }

    private static UpdateUserCommand ValidCommand(Guid id) => new()
    {
        Id = id,
        Username = "updatedname",
        Email = "updated@test.com",
        Password = PlainPassword,
        Phone = "+15551234567",
        Status = UserStatus.Active,
        Role = UserRole.Admin
    };

    [Fact(DisplayName = "Given an existing user When updating Then applies the changes, keeps Id and CreatedAt and stores the hashed password")]
    public async Task Handle_ExistingUser_UpdatesInPlaceAndHashesPassword()
    {
        // Given
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var existing = new User
        {
            Id = id,
            Username = "originalname",
            Email = "original@test.com",
            Password = "old-hash",
            Phone = "+15550000000",
            Status = UserStatus.Active,
            Role = UserRole.Customer,
            CreatedAt = createdAt
        };
        _userRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existing);

        // When
        var response = await _handler.Handle(ValidCommand(id), CancellationToken.None);

        // Then
        response.Success.Should().BeTrue();
        existing.Username.Should().Be("updatedname");
        existing.Email.Should().Be("updated@test.com");
        existing.Role.Should().Be(UserRole.Admin);
        existing.Password.Should().Be(HashedPassword);
        existing.Id.Should().Be(id);
        existing.CreatedAt.Should().Be(createdAt);
        existing.UpdatedAt.Should().NotBeNull();
        await _userRepository.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a non-existent user id When updating Then throws KeyNotFoundException and saves nothing")]
    public async Task Handle_UserNotFound_ThrowsKeyNotFound()
    {
        // Given
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        // When
        var act = () => _handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given an empty id When updating Then throws ValidationException before touching the repository")]
    public async Task Handle_EmptyId_ThrowsValidationException()
    {
        // When
        var act = () => _handler.Handle(ValidCommand(Guid.Empty), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        await _userRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
