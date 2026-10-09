using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace SalesApi.Application.Common;

/// <summary>
/// Decorator that keeps the cache optional: a cache failure never fails the request.
/// Reads degrade to a miss, writes are skipped, and a failed invalidation is logged as an
/// error, since the entry may stay stale until its TTL expires.
/// Cancellation is not a cache failure, so it still propagates.
/// </summary>
public class ResilientDistributedCache : IDistributedCache
{
    private readonly IDistributedCache _inner;
    private readonly ILogger<ResilientDistributedCache> _logger;

    public ResilientDistributedCache(IDistributedCache inner, ILogger<ResilientDistributedCache> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public byte[]? Get(string key)
    {
        try { return _inner.Get(key); }
        catch (Exception ex) when (ex is not OperationCanceledException) { return ReadFailed(ex, key); }
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        try { return await _inner.GetAsync(key, token); }
        catch (Exception ex) when (ex is not OperationCanceledException) { return ReadFailed(ex, key); }
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        try { _inner.Set(key, value, options); }
        catch (Exception ex) when (ex is not OperationCanceledException) { WriteFailed(ex, key); }
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        try { await _inner.SetAsync(key, value, options, token); }
        catch (Exception ex) when (ex is not OperationCanceledException) { WriteFailed(ex, key); }
    }

    public void Refresh(string key)
    {
        try { _inner.Refresh(key); }
        catch (Exception ex) when (ex is not OperationCanceledException) { WriteFailed(ex, key); }
    }

    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        try { await _inner.RefreshAsync(key, token); }
        catch (Exception ex) when (ex is not OperationCanceledException) { WriteFailed(ex, key); }
    }

    public void Remove(string key)
    {
        try { _inner.Remove(key); }
        catch (Exception ex) when (ex is not OperationCanceledException) { RemoveFailed(ex, key); }
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        try { await _inner.RemoveAsync(key, token); }
        catch (Exception ex) when (ex is not OperationCanceledException) { RemoveFailed(ex, key); }
    }

    private byte[]? ReadFailed(Exception ex, string key)
    {
        _logger.LogWarning(ex, "Cache read failed for {CacheKey}; treating as a miss", key);
        return null;
    }

    private void WriteFailed(Exception ex, string key) =>
        _logger.LogWarning(ex, "Cache write failed for {CacheKey}; skipping", key);

    private void RemoveFailed(Exception ex, string key) =>
        _logger.LogError(ex, "Cache invalidation failed for {CacheKey}; entry may be stale until its TTL expires", key);
}
