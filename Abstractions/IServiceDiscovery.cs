namespace Cjora.ServiceGovernance.Abstractions;

/// <summary>
/// 服务发现抽象接口
/// </summary>
public interface IServiceDiscovery
{
    /// <summary>
    /// 获取指定服务的所有可用实例
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>服务实例列表</returns>
    Task<IReadOnlyList<ServiceInstance>> GetInstancesAsync(
        string serviceName,
        CancellationToken cancellationToken = default);
}
