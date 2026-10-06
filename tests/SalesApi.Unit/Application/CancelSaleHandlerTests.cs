using SalesApi.Application.Sales.CancelSale;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Enums;
using SalesApi.Domain.Repositories;
using SalesApi.Unit.Domain.Entities.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace SalesApi.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CancelSaleHandler"/> class.
/// </summary>
public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new CancelSaleHandler(_saleRepository);
    }

    [Fact(DisplayName = "Given active sale When cancelling Then sale is cancelled and returns success")]
    public async Task Handle_ValidRequest_CancelsSaleAndReturnsSuccess()
    {
        // Given
        var sale = SaleTestData.GenerateValidSaleWithItem();
        var command = new CancelSaleCommand(sale.Id);

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Success.Should().BeTrue();
        sale.Status.Should().Be(SaleStatus.Cancelled);
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given sale that does not exist When cancelling Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new CancelSaleCommand(Guid.NewGuid());
        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given already cancelled sale When cancelling again Then throws InvalidOperationException")]
    public async Task Handle_AlreadyCancelledSale_ThrowsInvalidOperationException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSaleWithItem();
        sale.Cancel();
        sale.ClearEvents();
        var command = new CancelSaleCommand(sale.Id);

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
