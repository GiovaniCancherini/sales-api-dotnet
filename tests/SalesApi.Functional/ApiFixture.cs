using SalesApi.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Xunit;

namespace SalesApi.Functional;

/// <summary>
/// Boots the real WebApi host (full middleware pipeline: auth, exception
/// handling, validation, JSON serialization) against a real, throwaway
/// Postgres instance via Testcontainers - as close to hitting the actual
/// running API as an automated test gets, replacing the manual curl
/// verification this project relied on during development.
/// </summary>
public class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .WithDatabase("sales_api_functional")
        .WithUsername("developer")
        .WithPassword("***REMOVED***")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _container.GetConnectionString()
                });
            });
        });
    }

    // A fresh client per call, not one shared instance: tests mutate default
    // headers (Authorization) and must not leak state into one another, even
    // though the collection fixture already keeps them from running in parallel.
    public HttpClient CreateClient() => _factory.CreateClient();

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "Api";
}
