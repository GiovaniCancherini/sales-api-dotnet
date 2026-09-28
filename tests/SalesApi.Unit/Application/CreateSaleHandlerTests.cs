using SalesApi.Application.Sales.CreateSale;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;
using SalesApi.Domain.ValueObjects;
using SalesApi.Unit.Domain;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace SalesApi.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CreateSaleHandler"/> class.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateSaleHandler(_saleRepository, _mapper);
    }

    /// <summary>
    /// Tests that a valid sale creation request is handled successfully.
    /// </summary>
    [Fact(DisplayName = "Given valid sale data When creating sale Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var result = new CreateSaleResult { Id = Guid.NewGuid() };

        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(result);

        // When
        var createSaleResult = await _handler.Handle(command, CancellationToken.None);

        // Then
        createSaleResult.Should().NotBeNull();
        createSaleResult.Id.Should().Be(result.Id);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that an invalid sale creation request throws a validation exception.
    /// </summary>
    [Fact(DisplayName = "Given invalid sale data When creating sale Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Given
        var command = new CreateSaleCommand(); // Empty command fails validation

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    /// <summary>
    /// Regression guard: CreateSaleProfile used to have a broken
    /// CreateMap&lt;CreateSaleCommand, Sale&gt; that could never actually construct
    /// a Sale (private setters + a static Sale.Create factory). Proves the
    /// Handler builds the aggregate correctly from the command's flat data
    /// instead of relying on AutoMapper for that.
    /// </summary>
    [Fact(DisplayName = "Given valid command When handling Then builds Sale from command data correctly")]
    public async Task Handle_ValidRequest_ConstructsSaleFromCommand()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).CreateAsync(
            Arg.Is<Sale>(sale =>
                sale.SaleNumber == command.SaleNumber &&
                sale.Customer.Id == Guid.Parse(command.CustomerId) &&
                sale.Branch.Id == Guid.Parse(command.BranchId) &&
                sale.Items.Count == command.Items.Count),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that the Handler maps the entity the repository returns (post-save),
    /// not the one it built before calling the repository.
    /// </summary>
    [Fact(DisplayName = "Given created sale When handling Then maps the repository's returned entity")]
    public async Task Handle_ValidRequest_MapsCreatedSaleToResult()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var customer = new ExternalCustomer(Guid.Parse(command.CustomerId), command.CustomerName);
        var branch = new ExternalBranch(Guid.Parse(command.BranchId), command.BranchName, command.BranchCode);
        var persistedSale = Sale.Create(command.SaleNumber, customer, branch);

        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(persistedSale);
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult { Id = persistedSale.Id });

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Id.Should().Be(persistedSale.Id);
        _mapper.Received(1).Map<CreateSaleResult>(persistedSale);
    }
}
