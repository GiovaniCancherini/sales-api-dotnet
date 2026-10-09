using System.Text.Json;
using SalesApi.Application.Sales;
using SalesApi.Application.Sales.GetBySaleId;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;
using SalesApi.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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
    // Real in-memory IDistributedCache: same contract as Redis, no server needed.
    private readonly IDistributedCache _cache;
    private readonly GetBySaleIdHandler _handler;

    public GetBySaleIdHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        _handler = new GetBySaleIdHandler(_saleRepository, _mapper, _cache, NullLogger<GetBySaleIdHandler>.Instance);
    }

    [Fact(DisplayName = "Given cache miss When getting by id Then reads repository and caches the result")]
    public async Task Handle_CacheMiss_ReadsRepositoryAndCachesResult()
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
        var cached = JsonSerializer.Deserialize<GetBySaleIdResult>(_cache.GetString(SaleCacheKeys.ById(sale.Id))!);
        cached!.SaleNumber.Should().Be(sale.SaleNumber);
    }

    [Fact(DisplayName = "Given cache hit When getting by id Then returns cached result without touching repository")]
    public async Task Handle_CacheHit_DoesNotTouchRepository()
    {
        // Given
        var id = Guid.NewGuid();
        var cachedResult = new GetBySaleIdResult { Id = id, SaleNumber = "S-CACHED" };
        _cache.SetString(SaleCacheKeys.ById(id), JsonSerializer.Serialize(cachedResult));

        // When
        var result = await _handler.Handle(new GetBySaleIdQuery(id), CancellationToken.None);

        // Then
        result.SaleNumber.Should().Be("S-CACHED");
        await _saleRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _mapper.DidNotReceive().Map<GetBySaleIdResult>(Arg.Any<Sale>());
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
        _cache.GetString(SaleCacheKeys.ById(query.Id)).Should().BeNull();
    }

}
