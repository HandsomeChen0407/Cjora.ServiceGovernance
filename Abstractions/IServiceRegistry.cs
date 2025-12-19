namespace Cjora.ServiceGovernance.Abstractions;

/// <summary>
/// 服务注册抽象接口
/// 支持 Consul/Nacos 等不同注册中心实现
/// </summary>
public interface IServiceRegistry
{
    /// <summary>
    /// 注册服务
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task RegisterAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 注销服务
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeregisterAsync(CancellationToken cancellationToken = default);
}
