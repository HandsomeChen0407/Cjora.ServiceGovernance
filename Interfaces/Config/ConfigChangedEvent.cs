namespace Cjora.ServiceGovernance.Interfaces;

/// <summary>
/// 配置变更事件模型
/// 
/// 用途说明：
/// - 配置热更新（IOptions / 本地缓存）
/// - 事件驱动系统（配置变更通知）
/// - 审计 / 日志记录
/// 
/// 设计说明：
/// - 通过 IsDeleted 显式区分「删除」与「值为空」
/// - ChangedAt 为客户端感知时间（非配置中心真实提交时间）
/// </summary>
/// <param name="Key">
/// 配置 Key（不包含前缀）
/// 例如：redis / connectionStrings
/// </param>
/// <param name="Value">
/// 最新配置值
/// - 删除事件时为 null
/// - 非删除事件一定有值
/// </param>
/// <param name="Group">
/// 配置分组 / 命名空间
/// 用于隔离不同环境或业务域
/// </param>
/// <param name="ChangedAt">
/// 变更时间（UTC）
/// 注意：这是客户端收到变更的时间，不是配置中心的提交时间
/// </param>
/// <param name="IsDeleted">
/// 是否为删除事件
/// true  → 配置被删除
/// false → 配置新增 / 更新
/// </param>
public sealed record ConfigChangedEvent(
    string Key,
    string? Value,
    string Group,
    DateTime ChangedAt,
    bool IsDeleted = false);
