using System.Net;
using System.Net.Http.Json;
using SalesApi.Domain.Enums;
using SalesApi.WebApi.Common;
using SalesApi.WebApi.Features.Auth.AuthenticateUserFeature;
using SalesApi.WebApi.Features.Users.CreateUser;
using SalesApi.WebApi.Features.Users.GetUser;
using SalesApi.WebApi.Features.Users.ListUsers;
using SalesApi.WebApi.Features.Users.UpdateUser;
using Xunit;

namespace SalesApi.Functional;

/// <summary>
/// End-to-end coverage of the Users API through the real HTTP pipeline.
/// </summary>
[Collection(ApiCollection.Name)]
public class UsersFunctionalTests
{
    private readonly ApiFixture _fixture;

    public UsersFunctionalTests(ApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Given an existing user When updating it Then the change persists")]
    public async Task Given_ExistingUser_When_Updating_Then_ChangePersists()
    {
        // Given
        using var client = await AuthHelper.CreateAuthenticatedClientAsync(_fixture.CreateClient());
        var email = $"update-{Guid.NewGuid():N}@test.com";
        var createResponse = await client.PostAsJsonAsync("/api/Users", new CreateUserRequest
        {
            Username = "originalname",
            Email = email,
            Password = "Test@12345",
            Phone = "+15551234567",
            Status = UserStatus.Active,
            Role = UserRole.Customer
        });
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponseWithData<CreateUserResponse>>(JsonOptions.CaseInsensitive);

        // When
        var updateResponse = await client.PutAsJsonAsync("/api/Users", new UpdateUserRequest
        {
            Id = created!.Data!.Id,
            Username = "updatedname",
            Email = email,
            Password = "Test@12345",
            Phone = "+15551234567",
            Status = UserStatus.Active,
            Role = UserRole.Customer
        });

        // Then
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var getResponse = await client.GetAsync($"/api/Users/{created.Data.Id}");
        var detail = await getResponse.Content.ReadFromJsonAsync<ApiResponseWithData<GetUserResponse>>(JsonOptions.CaseInsensitive);
        Assert.Equal("updatedname", detail!.Data!.Name);

        // And - the password must still be stored hashed, not overwritten
        // with plaintext, or the user would be locked out after any update.
        using var anonymousClient = _fixture.CreateClient();
        var loginResponse = await anonymousClient.PostAsJsonAsync("/api/Auth", new AuthenticateUserRequest
        {
            Email = email,
            Password = "Test@12345"
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact(DisplayName = "Given a non-existent user id When updating it Then returns 404 ResourceNotFound")]
    public async Task Given_NonExistentUser_When_Updating_Then_Returns404()
    {
        // Given
        using var client = await AuthHelper.CreateAuthenticatedClientAsync(_fixture.CreateClient());

        // When
        var response = await client.PutAsJsonAsync("/api/Users", new UpdateUserRequest
        {
            Id = Guid.NewGuid(),
            Username = "doesnotexist",
            Email = $"missing-{Guid.NewGuid():N}@test.com",
            Password = "Test@12345",
            Phone = "+15551234567",
            Status = UserStatus.Active,
            Role = UserRole.Customer
        });

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions.CaseInsensitive);
        Assert.Equal("ResourceNotFound", body?.Type);
    }

    [Fact(DisplayName = "Given several users When listing with a multi-field order and page size Then the database returns one page with the total count")]
    public async Task Given_SeveralUsers_When_ListingWithOrderAndPageSize_Then_ReturnsOnePage()
    {
        // Given - the authenticated user plus two more guarantee at least three rows
        using var client = await AuthHelper.CreateAuthenticatedClientAsync(_fixture.CreateClient());
        for (var i = 0; i < 2; i++)
        {
            (await client.PostAsJsonAsync("/api/Users", new CreateUserRequest
            {
                Username = "listtest",
                Email = $"list-{Guid.NewGuid():N}@test.com",
                Password = "Test@12345",
                Phone = "+15551234567",
                Status = UserStatus.Active,
                Role = UserRole.Customer
            })).EnsureSuccessStatusCode();
        }

        // When - role and status are enums stored as strings, ordering by them must translate to SQL
        var response = await client.GetAsync("/api/Users?_order=role%20desc,%20status,%20email&_page=1&_size=2");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PaginatedResponse<ListUsersResponse>>(JsonOptions.CaseInsensitive);
        Assert.Equal(2, body!.Data!.Count());
        Assert.True(body.TotalCount >= 3);
        Assert.Equal((int)Math.Ceiling(body.TotalCount / 2.0), body.TotalPages);
    }
}
