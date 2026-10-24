using SalesApi.ORM.Repositories;
using SalesApi.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Security.Cryptography;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Xunit;

namespace SalesApi.Functional;

/// <summary>
/// Boots the real WebApi host (full middleware pipeline: auth, exception
/// handling, validation, JSON serialization) against real, throwaway
/// Postgres and MongoDB instances via Testcontainers - as close to hitting the
/// actual running API as an automated test gets, replacing the manual curl
/// verification this project relied on during development.
/// </summary>
public class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .WithDatabase("sales_api_functional")
        .WithUsername("developer")
        .Build();

    private readonly MongoDbContainer _mongo = new MongoDbBuilder()
        .WithImage("mongo:8.0")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;

    /// <summary>
    /// The collection the API appends sale events to, for asserting on them directly.
    /// </summary>
    public IMongoCollection<SaleEventDocument> SaleEvents { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _mongo.StartAsync());

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Redis",
                "localhost:6379,abortConnect=false,connectTimeout=1000,syncTimeout=1000,asyncTimeout=1000");
            builder.UseSetting("Jwt:SecretKey", Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)));
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
                    ["ConnectionStrings:MongoDb"] = _mongo.GetConnectionString()
                });
            });
        });

        SaleEvents = new MongoClient(_mongo.GetConnectionString())
            .GetDatabase(MongoSaleEventStore.DatabaseName)
            .GetCollection<SaleEventDocument>(MongoSaleEventStore.CollectionName);
    }

    // A fresh client per call, not one shared instance: tests mutate default
    // headers (Authorization) and must not leak state into one another, even
    // though the collection fixture already keeps them from running in parallel.
    public HttpClient CreateClient() => _factory.CreateClient();

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _mongo.DisposeAsync().AsTask());
    }
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "Api";
}
