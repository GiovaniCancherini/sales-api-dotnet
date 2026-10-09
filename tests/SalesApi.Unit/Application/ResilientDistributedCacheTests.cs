using SalesApi.Application.Common;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace SalesApi.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="ResilientDistributedCache"/> class.
/// </summary>
public class ResilientDistributedCacheTests
{
    private readonly IDistributedCache _inner = Substitute.For<IDistributedCache>();
    private readonly ResilientDistributedCache _cache;

    public ResilientDistributedCacheTests()
    {
        _cache = new ResilientDistributedCache(_inner, NullLogger<ResilientDistributedCache>.Instance);
    }

    [Fact(DisplayName = "Given cache is down When reading Then returns null as a miss")]
    public async Task GetAsync_CacheDown_ReturnsNull()
    {
        _inner.GetAsync("k", Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("down"));

        var result = await _cache.GetAsync("k");

        result.Should().BeNull();
    }

    [Fact(DisplayName = "Given cache is down When writing Then does not throw")]
    public async Task SetAsync_CacheDown_DoesNotThrow()
    {
        _inner.SetAsync("k", Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("down"));

        var act = () => _cache.SetAsync("k", new byte[] { 1 }, new DistributedCacheEntryOptions());

        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Given cache is down When invalidating Then does not throw")]
    public async Task RemoveAsync_CacheDown_DoesNotThrow()
    {
        _inner.RemoveAsync("k", Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("down"));

        var act = () => _cache.RemoveAsync("k");

        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Given request is cancelled When reading Then cancellation propagates")]
    public async Task GetAsync_Cancelled_Propagates()
    {
        _inner.GetAsync("k", Arg.Any<CancellationToken>()).ThrowsAsync(new OperationCanceledException());

        var act = () => _cache.GetAsync("k");

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact(DisplayName = "Given cache is up When reading Then returns the inner value")]
    public async Task GetAsync_CacheUp_ReturnsInnerValue()
    {
        var value = new byte[] { 42 };
        _inner.GetAsync("k", Arg.Any<CancellationToken>()).Returns(value);

        var result = await _cache.GetAsync("k");

        result.Should().BeSameAs(value);
    }
}
