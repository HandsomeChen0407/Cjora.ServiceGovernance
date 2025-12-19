namespace Cjora.ServiceGovernance.Service;

/// <summary>
/// 基于 Consul KV 的配置中心实现
/// </summary>
public sealed class ConsulConfigCenter : IConfigCenter
{
    private readonly IConsulClient _client;
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public ConsulConfigCenter(IConsulClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public async Task<string?> GetConfigAsync(string key, string? group = null, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var result = await _client.KV.Get(key, ct);
        if (result.Response?.Value == null)
            return null;

        var value = Encoding.UTF8.GetString(result.Response.Value);
        _cache[key] = value;
        return value;
    }

    /// <inheritdoc />
    public Task WatchConfigAsync(string key, Action<string> onChanged, string? group = null, CancellationToken ct = default)
    {
        _ = Task.Run(async () =>
        {
            ulong lastIndex = 0;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var queryOptions = new QueryOptions
                    {
                        WaitIndex = lastIndex,
                        WaitTime = TimeSpan.FromMinutes(5)
                    };

                    var result = await _client.KV.Get(key, queryOptions, ct);

                    if (result.LastIndex > lastIndex && result.Response?.Value != null)
                    {
                        lastIndex = result.LastIndex;
                        var value = Encoding.UTF8.GetString(result.Response.Value);
                        _cache[key] = value;
                        onChanged(value);
                    }
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), ct);
                }
            }
        }, ct);

        return Task.CompletedTask;
    }
}
