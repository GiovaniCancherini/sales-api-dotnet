using SalesApi.Application.Sales.CancelSaleItem;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;
using SalesApi.Unit.Domain.Entities.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace SalesApi.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CancelSaleItemHandler"/> class.
/// </summary>
public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new CancelSaleItemHandler(_saleRepository);
    }

    /// <summary>
    /// Regression guard: Sale.CancelItem used to leave Sale.TotalAmount stale
    /// after cancelling an item, because it never called RecalculateTotal.
    /// </summary>
    [Fact(DisplayName = "Given sale with two items When cancelling one Then sale total reflects only the remaining item")]
    public async Task Handle_ValidRequest_RecalculatesSaleTotalAfterCancellingItem()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale();
        var keptItem = SaleItemTestData.GenerateWithValues(1, 20m);
        var cancelledItem = SaleItemTestData.GenerateWithValues(1, 30m);
        sale.AddItem(keptItem);
        sale.AddItem(cancelledItem);
        sale.ClearEvents();

        var command = new CancelSaleItemCommand(sale.Id, cancelledItem.Id);

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Success.Should().BeTrue();
        cancelledItem.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(20m);
    }

    [Fact(DisplayName = "Given sale that does not exist When cancelling item Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new CancelSaleItemCommand(Guid.NewGuid(), Guid.NewGuid());
        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given item that does not belong to the sale When cancelling Then throws InvalidOperationException")]
    public async Task Handle_ItemNotFoundInSale_ThrowsInvalidOperationException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSaleWithItem();
        var command = new CancelSaleItemCommand(sale.Id, Guid.NewGuid());

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

}
