using SalesApi.Application.Sales.AddSaleItem;
using SalesApi.Application.Sales.CreateSale;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;
using SalesApi.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Xunit;

namespace SalesApi.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="AddSaleItemHandler"/> class.
/// </summary>
public class AddSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDistributedCache _cache = Substitute.For<IDistributedCache>();
    private readonly AddSaleItemHandler _handler;

    public AddSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new AddSaleItemHandler(_saleRepository, _cache);
    }

    private static CreateSaleItemCommand ValidItem(int quantity = 2) => new()
    {
        ProductId = Guid.NewGuid().ToString(),
        ProductName = "Refrigerante",
        ProductSku = "SKU-002",
        Quantity = quantity,
        UnitPrice = 10m
    };

    [Fact(DisplayName = "Given valid item When adding to sale Then item is added and returns success")]
    public async Task Handle_ValidRequest_AddsItemAndReturnsSuccess()
    {
        // Given
        var sale = SaleTestData.GenerateValidSaleWithItem();
        var command = new AddSaleItemCommand { SaleId = sale.Id, Item = ValidItem() };

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Success.Should().BeTrue();
        sale.Items.Should().HaveCount(2);
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Ties this handler to the real tiered discount rule: adding an item
    /// with quantity 5 (the 4-9 tier) should leave the sale's total 10% below
    /// the raw subtotal, not just the sum of quantity * unitPrice.
    /// </summary>
    [Fact(DisplayName = "Given item quantity in the 4-9 tier When adding Then sale total reflects the 10% discount")]
    public async Task Handle_ValidRequest_RecalculatesTotalWithDiscount()
    {
        // Given - fresh sale with no pre-existing item, so the total is unambiguous
        var sale = SaleTestData.GenerateValidSale();
        var command = new AddSaleItemCommand { SaleId = sale.Id, Item = ValidItem(quantity: 5) }; // subtotal 50, 10% off -> 45

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        sale.TotalAmount.Should().Be(45m);
    }

    [Fact(DisplayName = "Given sale that does not exist When adding item Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new AddSaleItemCommand { SaleId = Guid.NewGuid(), Item = ValidItem() };
        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

}
