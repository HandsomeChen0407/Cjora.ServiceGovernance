namespace Cjora.ServiceGovernance.Service;

/// <summary>
/// 基于 Consul KV 的配置中心实现
/// 
/// Key 映射规则：
/// /config/{group}/{key}
/// 
/// 设计特性：
/// - 本地缓存减少 Consul 访问
/// - Consul Watch 实现配置变更监听
/// - 支持透明加解密
/// - 最终一致性模型
/// </summary>
public sealed class ConsulConfigCenter : IConfigCenter
{
    private const string RootPrefix = "config";
    private const string DefaultGroup = "default";

    private readonly IConsulClient _client;
    private readonly IConfigEncryptor? _encryptor;

    /// <summary>
    /// 本地缓存（Key 为完整 Consul Key）
    ///
    /// 用途：
    /// - 减少频繁访问 Consul
    /// - Watch 去重（防止重复回调）
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _cache = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="client">Consul 客户端</param>
    /// <param name="encryptor">可选的配置加密器</param>
    public ConsulConfigCenter(
        IConsulClient client,
        IConfigEncryptor? encryptor = null)
    {
        _client = client;
        _encryptor = encryptor;
    }

    /// <summary>
    /// 获取指定配置
    /// 优先从本地缓存读取
    /// </summary>
    public async Task<string?> GetConfigAsync(
        string key,
        string? group = null,
        CancellationToken ct = default)
    {
        var fullKey = BuildKey(key, group);

        if (_cache.TryGetValue(fullKey, out var cached))
            return cached;

        var result = await _client.KV.Get(fullKey, ct);
        if (result.Response?.Value == null)
            return null;

        var value = Decode(result.Response.Value);
        _cache[fullKey] = value;
        return value;
    }

    /// <summary>
    /// 获取指定分组下的所有配置
    /// </summary>
    public async Task<IReadOnlyDictionary<string, string>> GetAllAsync(
        string? group = null,
        CancellationToken ct = default)
    {
        var prefix = BuildPrefix(group);
        var result = await _client.KV.List(prefix, ct);

        var dict = new Dictionary<string, string>();

        if (result.Response == null)
            return dict;

        foreach (var kv in result.Response)
        {
            if (kv.Value == null || !kv.Key.StartsWith(prefix))
                continue;

            var key = kv.Key.Substring(prefix.Length);
            var value = Decode(kv.Value);

            dict[key] = value;
            _cache[kv.Key] = value;
        }

        return dict;
    }

    /// <summary>
    /// 新增或更新配置
    /// 写入前自动进行加密（如启用）
    /// </summary>
    public async Task SetConfigAsync(
        string key,
        string value,
        string? group = null,
        CancellationToken ct = default)
    {
        if (_encryptor != null)
            value = _encryptor.Encrypt(value);

        var fullKey = BuildKey(key, group);

        var pair = new KVPair(fullKey)
        {
            Value = Encoding.UTF8.GetBytes(value)
        };

        var result = await _client.KV.Put(pair, ct);
        if (!result.Response)
            throw new InvalidOperationException($"Failed to set config: {fullKey}");

        _cache[fullKey] = value;
    }

    /// <summary>
    /// 删除配置
    /// </summary>
    public async Task DeleteConfigAsync(
        string key,
        string? group = null,
        CancellationToken ct = default)
    {
        var fullKey = BuildKey(key, group);

        var result = await _client.KV.Delete(fullKey, ct);
        if (!result.Response)
            throw new InvalidOperationException($"Failed to delete config: {fullKey}");

        _cache.TryRemove(fullKey, out _);
    }

    /// <summary>
    /// 监听指定配置 Key 的变更
    /// 使用 Consul KV 长轮询机制实现
    /// </summary>
    public Task<IDisposable> WatchConfigAsync(
        string key,
        Action<ConfigChangedEvent> onChanged,
        string? group = null,
        CancellationToken ct = default)
    {
        var fullKey = BuildKey(key, group);
        var actualGroup = group ?? DefaultGroup;

        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        _ = Task.Run(async () =>
        {
            ulong lastIndex = 0;

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    var options = new QueryOptions
                    {
                        WaitIndex = lastIndex,
                        WaitTime = TimeSpan.FromMinutes(5)
                    };

                    var result = await _client.KV.Get(fullKey, options, cts.Token);

                    if (result.LastIndex <= lastIndex)
                        continue;

                    lastIndex = result.LastIndex;

                    // 配置被删除
                    if (result.Response?.Value == null)
                    {
                        _cache.TryRemove(fullKey, out _);

                        onChanged(new ConfigChangedEvent(
                            key,
                            null,
                            actualGroup,
                            DateTime.UtcNow,
                            true));
                        continue;
                    }

                    var value = Decode(result.Response.Value);

                    // 值未变化，忽略
                    if (_cache.TryGetValue(fullKey, out var old) && old == value)
                        continue;

                    _cache[fullKey] = value;

                    onChanged(new ConfigChangedEvent(
                        key,
                        value,
                        actualGroup,
                        DateTime.UtcNow));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // 异常保护，避免 Watch 线程退出
                    await Task.Delay(3000, cts.Token);
                }
            }
        }, cts.Token);

        return Task.FromResult<IDisposable>(new WatchHandle(cts));
    }

    /// <summary>
    /// Watch 取消句柄
    /// </summary>
    private sealed class WatchHandle : IDisposable
    {
        private readonly CancellationTokenSource _cts;
        public WatchHandle(CancellationTokenSource cts) => _cts = cts;
        public void Dispose() => _cts.Cancel();
    }

    private static string BuildKey(string key, string? group)
        => $"{RootPrefix}/{group ?? DefaultGroup}/{key}";

    private static string BuildPrefix(string? group)
        => $"{RootPrefix}/{group ?? DefaultGroup}/";

    /// <summary>
    /// 解码并解密配置值
    /// </summary>
    private string Decode(byte[] value)
    {
        var text = Encoding.UTF8.GetString(value);

        if (_encryptor != null && _encryptor.IsEncrypted(text))
            return _encryptor.Decrypt(text);

        return text;
    }
}
