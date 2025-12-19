namespace Cjora.ServiceGovernance.Abstractions;

/// <summary>
/// 服务注册抽象接口
/// 企业级场景要求：
/// - 支持应用启动注册服务
/// - 支持应用停止注销服务
/// - 支持心跳防止服务雪崩
/// </summary>
public interface IServiceRegistry
{
    /// <summary>
    /// 注册服务
    /// </summary>
    Task RegisterAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 注销服务
    /// </summary>
    Task DeregisterAsync(CancellationToken cancellationToken = default);
}
