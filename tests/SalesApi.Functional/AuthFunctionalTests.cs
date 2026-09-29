using System.Net;
using System.Net.Http.Json;
using SalesApi.Domain.Enums;
using SalesApi.WebApi.Common;
using SalesApi.WebApi.Features.Auth.AuthenticateUserFeature;
using SalesApi.WebApi.Features.Users.CreateUser;
using Xunit;

namespace SalesApi.Functional;

/// <summary>
/// End-to-end coverage of the authentication bootstrap flow and of
/// enforcement on a protected endpoint - through the real HTTP pipeline
/// (JWT middleware, exception handling, JSON serialization), not a mock.
/// </summary>
[Collection(ApiCollection.Name)]
public class AuthFunctionalTests
{
    private readonly ApiFixture _fixture;

    public AuthFunctionalTests(ApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Given no seed data When creating a user anonymously Then it succeeds with every field populated")]
    public async Task Given_NoToken_When_CreatingUser_Then_Returns201WithPopulatedFields()
    {
        // Given
        using var client = _fixture.CreateClient();
        var email = $"auth-{Guid.NewGuid():N}@test.com";

        // When
        var response = await client.PostAsJsonAsync("/api/Users", new CreateUserRequest
        {
            Username = "authtest",
            Email = email,
            Password = "Test@12345",
            Phone = "+15551234567",
            Status = UserStatus.Active,
            Role = UserRole.Customer
        });

        // Then
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWithData<CreateUserResponse>>(JsonOptions.CaseInsensitive);
        Assert.NotNull(body?.Data);
        Assert.Equal("authtest", body.Data.Name);
        Assert.Equal(email, body.Data.Email);
    }

    [Fact(DisplayName = "Given valid credentials When logging in Then returns a usable token")]
    public async Task Given_ValidCredentials_When_LoggingIn_Then_ReturnsToken()
    {
        // Given
        using var client = _fixture.CreateClient();
        await AuthHelper.CreateAuthenticatedClientAsync(client); // exercises the full create+login flow

        // Then - the helper already asserts success internally; a protected
        // call proves the returned token is actually accepted downstream.
        var response = await client.GetAsync("/api/Sales");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "Given wrong password When logging in Then returns 401 AuthenticationError")]
    public async Task Given_WrongPassword_When_LoggingIn_Then_Returns401()
    {
        // Given
        using var client = _fixture.CreateClient();
        var email = $"auth-{Guid.NewGuid():N}@test.com";
        await client.PostAsJsonAsync("/api/Users", new CreateUserRequest
        {
            Username = "wrongpwtest",
            Email = email,
            Password = "Test@12345",
            Phone = "+15551234567",
            Status = UserStatus.Active,
            Role = UserRole.Customer
        });

        // When
        var response = await client.PostAsJsonAsync("/api/Auth", new AuthenticateUserRequest
        {
            Email = email,
            Password = "WrongPassword"
        });

        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions.CaseInsensitive);
        Assert.Equal("AuthenticationError", body?.Type);
    }

    [Fact(DisplayName = "Given no token When calling a protected endpoint Then returns 401 with the standard error shape")]
    public async Task Given_NoToken_When_CallingProtectedEndpoint_Then_Returns401WithErrorShape()
    {
        // Given
        using var client = _fixture.CreateClient();

        // When
        var response = await client.GetAsync("/api/Sales");

        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions.CaseInsensitive);
        Assert.Equal("AuthenticationError", body?.Type);
        Assert.NotEmpty(body!.Detail);
    }
}
