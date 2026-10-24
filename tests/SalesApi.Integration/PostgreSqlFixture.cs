using Testcontainers.PostgreSql;
using Xunit;

namespace SalesApi.Integration;

/// <summary>
/// Spins up a real, throwaway Postgres instance via Testcontainers for the
/// whole Integration test collection - shared by all tests in the collection
/// so the container starts once per test run, not once per test class.
/// </summary>
public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .WithDatabase("sales_api_test")
        .WithUsername("developer")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition(Name)]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "Postgres";
}
