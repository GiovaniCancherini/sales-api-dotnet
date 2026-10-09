using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.Sales.GetBySaleId;

/// <summary>
/// Handler for processing GetBySaleIdQuery requests, with a cache-aside read through IDistributedCache.
/// </summary>
public class GetBySaleIdHandler : IRequestHandler<GetBySaleIdQuery, GetBySaleIdResult>
{
    // Writes invalidate the key, so the TTL is only a safety net if an invalidation is lost.
    private static readonly DistributedCacheEntryOptions CacheEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetBySaleIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of GetBySaleIdHandler
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="cache">The distributed cache (Redis in production)</param>
    /// <param name="logger">The logger</param>
    public GetBySaleIdHandler(
        ISaleRepository saleRepository,
        IMapper mapper,
        IDistributedCache cache,
        ILogger<GetBySaleIdHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetBySaleIdQuery request
    /// </summary>
    /// <param name="request">The GetBySaleId query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale details if found</returns>
    public async Task<GetBySaleIdResult> Handle(GetBySaleIdQuery request, CancellationToken cancellationToken)
    {
        var key = SaleCacheKeys.ById(request.Id);

        var cached = await _cache.GetStringAsync(key, cancellationToken);
        if (cached != null)
        {
            _logger.LogInformation("Cache hit for {CacheKey}", key);
            return JsonSerializer.Deserialize<GetBySaleIdResult>(cached)!;
        }

        _logger.LogInformation("Cache miss for {CacheKey}", key);

        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sale == null)
        {
            // Not found is not cached: a sale created right after would be hidden until the TTL expires.
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");
        }

        var result = _mapper.Map<GetBySaleIdResult>(sale);
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(result), CacheEntryOptions, cancellationToken);

        return result;
    }
}
