using SalesApi.Domain.Entities;
using SalesApi.Domain.ValueObjects;
using SalesApi.ORM;
using SalesApi.ORM.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace SalesApi.Integration;

/// <summary>
/// Exercises SaleRepository against a real Postgres instance. These scenarios
/// specifically cover the EF Core Added-vs-Modified ambiguity that a mocked
/// ISaleRepository (as used everywhere in the Unit test suite) can never
/// catch: SaleItem generates its own client-side Guid, which made EF
/// misjudge new items as already-persisted ones and throw
/// DbUpdateConcurrencyException. That bug only ever manifested against a
/// real database - it's exactly what this test class exists to guard against.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
public class SaleRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private DefaultContext _context = null!;
    private SaleRepository _repository = null!;

    public SaleRepositoryTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseNpgsql(_fixture.ConnectionString, b => b.MigrationsAssembly("SalesApi.ORM"))
            .Options;

        _context = new DefaultContext(options);
        await _context.Database.MigrateAsync();

        _repository = new SaleRepository(_context);
    }

    public Task DisposeAsync() => _context.DisposeAsync().AsTask();

    private static Sale CreateValidSale(string saleNumber)
    {
        var customer = new ExternalCustomer(Guid.NewGuid(), "Integration Test Customer");
        var branch = new ExternalBranch(Guid.NewGuid(), "Integration Test Branch", "ITB");
        return Sale.Create(saleNumber, customer, branch);
    }

    private static SaleItem CreateValidItem(int quantity, decimal unitPrice)
    {
        var product = new ExternalProduct(Guid.NewGuid(), "Integration Test Product", "ITP-001");
        return new SaleItem(product, quantity, unitPrice);
    }

    [Fact(DisplayName = "Given a new sale When created Then it can be retrieved with its item")]
    public async Task Given_NewSale_When_Created_Then_CanBeRetrieved()
    {
        // Given
        var sale = CreateValidSale($"INT-{Guid.NewGuid()}");
        sale.AddItem(CreateValidItem(2, 10m));

        // When
        await _repository.CreateAsync(sale);
        var retrieved = await _repository.GetByIdAsync(sale.Id);

        // Then
        Assert.NotNull(retrieved);
        Assert.Equal(sale.SaleNumber, retrieved.SaleNumber);
        Assert.Equal(20m, retrieved.TotalAmount);
        Assert.Single(retrieved.Items);
    }

    [Fact(DisplayName = "Given an existing sale When adding a new item Then it is inserted, not mistaken for an update")]
    public async Task Given_ExistingSale_When_AddingNewItem_Then_PersistsCorrectly()
    {
        // Given
        var sale = CreateValidSale($"INT-{Guid.NewGuid()}");
        sale.AddItem(CreateValidItem(1, 10m));
        await _repository.CreateAsync(sale);

        // When - load fresh (separate tracked graph, like the real handlers do)
        var loaded = await _repository.GetByIdAsync(sale.Id);
        Assert.NotNull(loaded);
        loaded.AddItem(CreateValidItem(1, 5m));
        await _repository.UpdateAsync(loaded);

        // Then
        var reloaded = await _repository.GetByIdAsync(sale.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(2, reloaded.Items.Count);
        Assert.Equal(15m, reloaded.TotalAmount);
    }

    [Fact(DisplayName = "Given an existing sale When replacing its items Then old items are removed and new ones inserted")]
    public async Task Given_ExistingSale_When_ReplacingItems_Then_OldItemsRemovedNewItemsInserted()
    {
        // Given
        var sale = CreateValidSale($"INT-{Guid.NewGuid()}");
        var originalItem = CreateValidItem(1, 20m);
        sale.AddItem(originalItem);
        await _repository.CreateAsync(sale);

        // When
        var loaded = await _repository.GetByIdAsync(sale.Id);
        Assert.NotNull(loaded);
        loaded.ReplaceItems(new[] { CreateValidItem(1, 5m), CreateValidItem(1, 7m) });
        await _repository.UpdateAsync(loaded);

        // Then
        var reloaded = await _repository.GetByIdAsync(sale.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(2, reloaded.Items.Count);
        Assert.DoesNotContain(reloaded.Items, i => i.Id == originalItem.Id);
        Assert.Equal(12m, reloaded.TotalAmount);
    }

    [Fact(DisplayName = "Given an existing sale When cancelling one item Then the sale total reflects only what remains")]
    public async Task Given_ExistingSale_When_CancellingItem_Then_TotalReflectsRemainingItem()
    {
        // Given
        var sale = CreateValidSale($"INT-{Guid.NewGuid()}");
        var keptItem = CreateValidItem(1, 20m);
        var cancelledItem = CreateValidItem(1, 30m);
        sale.AddItem(keptItem);
        sale.AddItem(cancelledItem);
        await _repository.CreateAsync(sale);

        // When
        var loaded = await _repository.GetByIdAsync(sale.Id);
        Assert.NotNull(loaded);
        loaded.CancelItem(cancelledItem.Id);
        await _repository.UpdateAsync(loaded);

        // Then
        var reloaded = await _repository.GetByIdAsync(sale.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(20m, reloaded.TotalAmount);
        Assert.True(reloaded.Items.Single(i => i.Id == cancelledItem.Id).IsCancelled);
    }

    [Fact(DisplayName = "Given an existing sale When deleted Then it can no longer be retrieved")]
    public async Task Given_ExistingSale_When_Deleted_Then_CannotBeRetrieved()
    {
        // Given
        var sale = CreateValidSale($"INT-{Guid.NewGuid()}");
        await _repository.CreateAsync(sale);

        // When
        var deleted = await _repository.DeleteAsync(sale.Id);

        // Then
        Assert.True(deleted);
        Assert.Null(await _repository.GetByIdAsync(sale.Id));
    }
}
