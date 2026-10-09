using SalesApi.Application.Sales.CreateSale;
using SalesApi.Application.Sales.UpdateSale;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;
using SalesApi.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Xunit;

namespace SalesApi.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSaleHandler"/> class.
/// </summary>
public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDistributedCache _cache = Substitute.For<IDistributedCache>();
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new UpdateSaleHandler(_saleRepository, _cache);
    }

    private static CreateSaleItemCommand ValidItem(int quantity = 2) => new()
    {
        ProductId = Guid.NewGuid().ToString(),
        ProductName = "Refrigerante",
        ProductSku = "SKU-002",
        Quantity = quantity,
        UnitPrice = 10m
    };

    [Fact(DisplayName = "Given valid command When updating sale Then replaces items and returns success")]
    public async Task Handle_ValidRequest_ReplacesItemsAndReturnsSuccess()
    {
        // Given
        var sale = SaleTestData.GenerateValidSaleWithItem();
        var command = new UpdateSaleCommand { Id = sale.Id, Items = new List<CreateSaleItemCommand> { ValidItem() } };

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Success.Should().BeTrue();
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Also guards against the same class of bug fixed in Sale.CancelItem
    /// (RecalculateTotal missing): replacing the item list must leave
    /// Sale.TotalAmount reflecting only the new items, not stale from the old ones.
    /// </summary>
    [Fact(DisplayName = "Given command replacing items When handling Then old items are replaced and total reflects only the new ones")]
    public async Task Handle_ValidRequest_ReplacesOldItemsWithNewOnes()
    {
        // Given - original item has a different price (30) than the new one (20),
        // so a stale total would be caught by the assertion below.
        var sale = SaleTestData.GenerateValidSale();
        var originalItem = SaleItemTestData.GenerateWithValues(1, 30m);
        sale.AddItem(originalItem);
        var command = new UpdateSaleCommand { Id = sale.Id, Items = new List<CreateSaleItemCommand> { ValidItem() } }; // qty 2 * 10 = 20

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        sale.Items.Should().ContainSingle();
        sale.Items.Single().Id.Should().NotBe(originalItem.Id);
        sale.TotalAmount.Should().Be(20m);
    }

    [Fact(DisplayName = "Given sale that does not exist When updating Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new UpdateSaleCommand { Id = Guid.NewGuid(), Items = new List<CreateSaleItemCommand> { ValidItem() } };
        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

}
