using SalesApi.Application;
using SalesApi.Application.Users.CreateUser;
using SalesApi.Application.Users.GetAllUser;
using SalesApi.Application.Users.GetUser;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Enums;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace SalesApi.Unit.Application;

/// <summary>
/// Validates the real AutoMapper configuration for the Application layer.
/// Every handler test mocks IMapper (Substitute.For&lt;IMapper&gt;()), so none of
/// them ever exercise the actual profiles - which is exactly how CreateUserResult
/// shipped with only an Id property, and how GetUserResult/GetAllUserResult
/// silently mapped Name as empty (User.Username doesn't match "Name" by
/// convention), both undetected by the handler test suite. A real mapper,
/// mapping a fully populated User, is the only way to catch either class of bug.
/// </summary>
public class AutoMapperProfilesTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(ApplicationLayer).Assembly), NullLoggerFactory.Instance);
        return config.CreateMapper();
    }

    private static User CreateFullyPopulatedUser() => new()
    {
        Id = Guid.NewGuid(),
        Username = "johndoe",
        Email = "john@doe.com",
        Phone = "+15551234567",
        Role = UserRole.Admin,
        Status = UserStatus.Active
    };

    [Fact(DisplayName = "Given the Application layer's AutoMapper profiles When validating Then configuration should be valid")]
    public void Configuration_IsValid()
    {
        // Given
        var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(ApplicationLayer).Assembly), NullLoggerFactory.Instance);

        // Then - throws AutoMapperConfigurationException if any destination
        // member has no matching source and no explicit ForMember
        config.AssertConfigurationIsValid();
    }

    [Fact(DisplayName = "Given a user When mapping to CreateUserResult Then every field is populated")]
    public void Map_UserToCreateUserResult_PopulatesAllFields()
    {
        // Given
        var mapper = CreateMapper();
        var user = CreateFullyPopulatedUser();

        // When
        var result = mapper.Map<CreateUserResult>(user);

        // Then
        result.Id.Should().Be(user.Id);
        result.Name.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.Phone.Should().Be(user.Phone);
        result.Role.Should().Be(user.Role);
        result.Status.Should().Be(user.Status);
    }

    [Fact(DisplayName = "Given a user When mapping to GetUserResult Then every field is populated")]
    public void Map_UserToGetUserResult_PopulatesAllFields()
    {
        // Given
        var mapper = CreateMapper();
        var user = CreateFullyPopulatedUser();

        // When
        var result = mapper.Map<GetUserResult>(user);

        // Then
        result.Id.Should().Be(user.Id);
        result.Name.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.Phone.Should().Be(user.Phone);
        result.Role.Should().Be(user.Role);
        result.Status.Should().Be(user.Status);
    }

    [Fact(DisplayName = "Given a user When mapping to GetAllUserResult Then every field is populated")]
    public void Map_UserToGetAllUserResult_PopulatesAllFields()
    {
        // Given
        var mapper = CreateMapper();
        var user = CreateFullyPopulatedUser();

        // When
        var result = mapper.Map<GetAllUserResult>(user);

        // Then
        result.Id.Should().Be(user.Id);
        result.Name.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.Phone.Should().Be(user.Phone);
        result.Role.Should().Be(user.Role);
        result.Status.Should().Be(user.Status);
    }
}
