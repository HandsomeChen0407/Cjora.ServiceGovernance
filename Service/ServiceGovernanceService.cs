namespace Cjora.ServiceGovernance.Service;

/// <summary>
/// 服务治理统一入口
/// 业务层依赖此类获取服务地址
/// </summary>
public sealed class ServiceGovernanceService : IServiceGovernance
{
    private readonly IServiceDiscovery _discovery;
    private readonly ILoadBalancer _loadBalancer;

    public ServiceGovernanceService(IServiceDiscovery discovery, ILoadBalancer loadBalancer)
    {
        _discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
        _loadBalancer = loadBalancer ?? throw new ArgumentNullException(nameof(loadBalancer));
    }

    /// <inheritdoc />
    public async Task<Uri> GetServiceUriAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("serviceName 不能为空", nameof(serviceName));

        var instances = await _discovery.GetInstancesAsync(serviceName, cancellationToken)
                            ?? throw new InvalidOperationException($"服务 {serviceName} 没有可用实例");

        var instance = _loadBalancer.Select(instances, serviceName)
                        ?? throw new InvalidOperationException($"负载均衡未选择到服务 {serviceName}");

        return instance.ToUri();
    }
}
