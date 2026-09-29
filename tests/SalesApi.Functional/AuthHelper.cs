using System.Net.Http.Headers;
using System.Net.Http.Json;
using SalesApi.Domain.Enums;
using SalesApi.WebApi.Common;
using SalesApi.WebApi.Features.Auth.AuthenticateUserFeature;
using SalesApi.WebApi.Features.Users.CreateUser;

namespace SalesApi.Functional;

/// <summary>
/// Creates a user and logs in, returning a ready-to-use authenticated
/// HttpClient - every protected endpoint test needs this same bootstrap
/// (there's no seed data, so a real account has to exist first).
/// </summary>
internal static class AuthHelper
{
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(HttpClient client)
    {
        var email = $"functional-{Guid.NewGuid():N}@test.com";
        const string password = "Test@12345";

        var createResponse = await client.PostAsJsonAsync("/api/Users", new CreateUserRequest
        {
            Username = $"functional-{Guid.NewGuid():N}",
            Email = email,
            Password = password,
            Phone = "+15551234567",
            Status = UserStatus.Active,
            Role = UserRole.Customer
        });
        createResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/api/Auth", new AuthenticateUserRequest
        {
            Email = email,
            Password = password
        });
        loginResponse.EnsureSuccessStatusCode();

        var body = await loginResponse.Content.ReadFromJsonAsync<ApiResponseWithData<AuthenticateUserResponse>>(JsonOptions.CaseInsensitive);
        var token = body!.Data!.Token;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
