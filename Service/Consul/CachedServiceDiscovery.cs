namespace Cjora.ServiceGovernance.Service;

/// <summary>
/// 缓存的服务发现
/// 避免频繁调用 Consul/Nacos
/// </summary>
public sealed class CachedServiceDiscovery : IServiceDiscovery
{
    private readonly IServiceDiscovery _inner;
    private readonly int _cacheSeconds;
    private readonly ConcurrentDictionary<string, (DateTimeOffset refreshAt, IReadOnlyList<ServiceInstance> instances)> _cache = new();

    public CachedServiceDiscovery(IServiceDiscovery inner, IOptions<ServiceGovernanceOptions> options)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        if (options == null) throw new ArgumentNullException(nameof(options));
        _cacheSeconds = options.Value?.Discovery?.CacheSeconds > 0 ? options.Value.Discovery.CacheSeconds : 10;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ServiceInstance>> GetInstancesAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("serviceName 不能为空", nameof(serviceName));

        if (_cache.TryGetValue(serviceName, out var cached))
        {
            if (DateTimeOffset.UtcNow < cached.refreshAt)
            {
                return cached.instances;
            }
        }

        var instances = await _inner.GetInstancesAsync(serviceName, cancellationToken)
                        ?? Array.Empty<ServiceInstance>();

        _cache[serviceName] = (DateTimeOffset.UtcNow.AddSeconds(_cacheSeconds), instances);
        return instances;
    }
}
