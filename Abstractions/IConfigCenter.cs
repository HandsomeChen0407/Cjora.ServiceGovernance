namespace Cjora.ServiceGovernance.Abstractions;

/// <summary>
/// 配置中心服务接口
/// 支持 Consul KV、Nacos Config 等实现
/// </summary>
public interface IConfigCenter
{
    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="key">配置 key</param>
    /// <param name="group">可选分组</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>配置值，如果不存在返回 null</returns>
    Task<string?> GetConfigAsync(string key, string? group = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置配置
    /// </summary>
    /// <param name="key">配置 key</param>
    /// <param name="value">配置值</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>配置值，如果不存在返回 null</returns>
    Task SetConfigAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 监听配置变更（本地缓存自动刷新）
    /// </summary>
    /// <param name="key">配置 key</param>
    /// <param name="onChanged">配置变更回调</param>
    /// <param name="group">可选分组</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task WatchConfigAsync(string key, Action<string> onChanged, string? group = null, CancellationToken cancellationToken = default);
}
