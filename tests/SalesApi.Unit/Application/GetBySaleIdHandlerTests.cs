using SalesApi.Application.Sales.GetBySaleId;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;
using SalesApi.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace SalesApi.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="GetBySaleIdHandler"/> class.
/// </summary>
public class GetBySaleIdHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetBySaleIdHandler _handler;

    public GetBySaleIdHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetBySaleIdHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given existing sale When getting by id Then returns mapped result")]
    public async Task Handle_ValidRequest_ReturnsMappedSale()
    {
        // Given
        var sale = SaleTestData.GenerateValidSaleWithItem();
        var query = new GetBySaleIdQuery(sale.Id);
        var expectedResult = new GetBySaleIdResult { Id = sale.Id, SaleNumber = sale.SaleNumber };

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetBySaleIdResult>(sale).Returns(expectedResult);

        // When
        var result = await _handler.Handle(query, CancellationToken.None);

        // Then
        result.Should().BeSameAs(expectedResult);
        _mapper.Received(1).Map<GetBySaleIdResult>(sale);
    }

    [Fact(DisplayName = "Given sale that does not exist When getting by id Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        // Given
        var query = new GetBySaleIdQuery(Guid.NewGuid());
        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given empty id When getting by id Then throws ValidationException")]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Given
        var query = new GetBySaleIdQuery(Guid.Empty);

        // When
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
    }
}
