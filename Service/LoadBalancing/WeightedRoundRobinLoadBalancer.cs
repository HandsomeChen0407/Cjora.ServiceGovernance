namespace Cjora.ServiceGovernance.Service;

/// <summary>
/// 加权轮询负载均衡实现
/// </summary>
public sealed class WeightedRoundRobinLoadBalancer : ILoadBalancer
{
    private readonly ConcurrentDictionary<string, int> _counters = new();

    /// <inheritdoc />
    public ServiceInstance Select(IReadOnlyList<ServiceInstance> instances, string serviceName)
    {
        if (instances == null || !instances.Any())
            throw new ArgumentException($"服务 {serviceName} 没有可用实例");

        // 构建加权列表
        var weightedList = instances.SelectMany(i => Enumerable.Repeat(i, Math.Max(i.Weight, 1))).ToList();

        // 轮询选择
        var count = _counters.GetOrAdd(serviceName, 0);
        var selected = weightedList[count++ % weightedList.Count];
        _counters[serviceName] = count;
        return selected;
    }
}
