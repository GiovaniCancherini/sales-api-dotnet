using System.Net;
using System.Net.Http.Json;
using SalesApi.Domain.Enums;
using SalesApi.WebApi.Common;
using SalesApi.WebApi.Features.Auth.AuthenticateUserFeature;
using SalesApi.WebApi.Features.Users.CreateUser;
using SalesApi.WebApi.Features.Users.GetUser;
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
}
