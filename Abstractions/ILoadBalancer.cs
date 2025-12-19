namespace Cjora.ServiceGovernance.Abstractions;

/// <summary>
/// 负载均衡算法接口
/// </summary>
public interface ILoadBalancer
{
    /// <summary>
    /// 从实例列表中选择一个实例
    /// </summary>
    /// <param name="instances">可用服务实例列表</param>
    /// <param name="serviceName">服务名称（用于日志或异常信息）</param>
    /// <returns>选中的服务实例</returns>
    ServiceInstance Select(IReadOnlyList<ServiceInstance> instances, string serviceName);
}
