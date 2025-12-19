namespace Cjora.ServiceGovernance.Service;

/// <summary>
/// 带缓存的服务发现
/// 
/// 核心作用：
/// - 所有业务请求 ≠ 所有请求打 Consul
/// - 控制刷新频率
/// </summary>
public sealed class CachedServiceDiscovery : IServiceDiscovery
{
    private readonly IServiceDiscovery _inner;
    private readonly int _cacheSeconds;

    private readonly ConcurrentDictionary<string, CacheItem> _cache = new();

    public CachedServiceDiscovery(
        IServiceDiscovery inner,
        IOptions<ServiceGovernanceOptions> options)
    {
        _inner = inner;
        _cacheSeconds = Math.Max(5, options.Value.Discovery.CacheSeconds);
    }

    public async Task<IReadOnlyList<ServiceInstance>> GetInstancesAsync(
        string serviceName,
        CancellationToken ct = default)
    {
        if (_cache.TryGetValue(serviceName, out var cached) &&
            cached.ExpireAt > DateTimeOffset.UtcNow)
        {
            return cached.Instances;
        }

        var instances = await _inner.GetInstancesAsync(serviceName, ct);

        _cache[serviceName] = new CacheItem(
            instances,
            DateTimeOffset.UtcNow.AddSeconds(_cacheSeconds));

        return instances;
    }

    private sealed record CacheItem(
        IReadOnlyList<ServiceInstance> Instances,
        DateTimeOffset ExpireAt);
}
