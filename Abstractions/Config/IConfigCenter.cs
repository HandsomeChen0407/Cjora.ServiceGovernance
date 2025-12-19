namespace Cjora.ServiceGovernance.Abstractions;

/// <summary>
/// 配置中心统一抽象接口
/// 
/// 设计目标：
/// 1. 屏蔽底层配置中心实现差异（Consul / Nacos / Etcd）
/// 2. 提供统一 CRUD 能力
/// 3. 支持配置监听（Watch）用于动态刷新
/// 4. 支持分组 / 命名空间隔离
/// 
/// 一致性说明：
/// - 本接口遵循「最终一致性」模型
/// - Watch 通过长轮询 / 推送感知变更
/// </summary>
public interface IConfigCenter
{
    /// <summary>
    /// 获取指定 Key 的配置值
    /// </summary>
    /// <param name="key">配置 Key（不包含前缀）</param>
    /// <param name="group">配置分组 / 命名空间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>
    /// 配置值
    /// - 不存在时返回 null
    /// </returns>
    Task<string?> GetConfigAsync(
        string key,
        string? group = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定分组下的所有配置
    /// 
    /// 常用于：
    ///  - 配置管理 UI
    ///  - 批量导出
    ///  - 启动时全量加载
    /// </summary>
    /// <param name="group">配置分组</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Key-Value 配置字典</returns>
    Task<IReadOnlyDictionary<string, string>> GetAllAsync(
        string? group = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增或更新配置
    /// 
    /// 行为说明：
    ///  - Key 不存在 → 新增
    ///  - Key 已存在 → 覆盖更新
    /// </summary>
    Task SetConfigAsync(
        string key,
        string value,
        string? group = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除指定配置
    /// 
    /// 行为说明：
    ///  - 删除后 Watch 一定会收到 IsDeleted = true 的事件
    /// </summary>
    Task DeleteConfigAsync(
        string key,
        string? group = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 监听指定配置 Key 的变更
    /// 
    /// 使用说明：
    ///  - 返回 IDisposable，用于主动取消监听
    ///  - 推荐在应用生命周期内保持监听
    ///  - 同一个 Key 可被多次监听
    /// </summary>
    Task<IDisposable> WatchConfigAsync(
        string key,
        Action<ConfigChangedEvent> onChanged,
        string? group = null,
        CancellationToken cancellationToken = default);
}
