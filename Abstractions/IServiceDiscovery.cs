namespace Cjora.ServiceGovernance.Abstractions;

/// <summary>
/// 服务发现抽象接口
/// 支持 Consul/Nacos/Etcd 等注册中心
/// 注意：高可用场景需考虑缓存、批量刷新、失败重试
/// </summary>
public interface IServiceDiscovery
{
    /// <summary>
    /// 获取指定服务的所有可用实例
    /// 返回的实例需支持临时停用（Weight = 0 表示停用）
    /// </summary>
    Task<IReadOnlyList<ServiceInstance>> GetInstancesAsync(string serviceName, CancellationToken cancellationToken = default);
}
