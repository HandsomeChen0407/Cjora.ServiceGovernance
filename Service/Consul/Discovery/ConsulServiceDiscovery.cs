namespace Cjora.ServiceGovernance.Service;

/// <summary>
/// Consul 服务发现实现
/// </summary>
public sealed class ConsulServiceDiscovery : IServiceDiscovery
{
    private readonly IConsulClient _client;

    public ConsulServiceDiscovery(IConsulClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ServiceInstance>> GetInstancesAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        var result = await _client.Health.Service(
            serviceName,
            tag: null,
            passingOnly: true,
            cancellationToken);

        if (result.Response == null || result.Response.Length == 0)
            return Array.Empty<ServiceInstance>();

        var instances = new List<ServiceInstance>();

        foreach (var entry in result.Response)
        {
            var service = entry.Service;
            if (service == null) continue;

            // 获取权重
            int weight = 1;
            if (service.Meta != null)
            {
                if (service.Meta.TryGetValue("Weight", out var wStr) && int.TryParse(wStr, out var w))
                    weight = w;
                else if (service.Meta.TryGetValue("weight", out var wStr2) && int.TryParse(wStr2, out var w2))
                    weight = w2;
            }

            // 如果 Address 为空，使用 Node.Address
            var address = string.IsNullOrWhiteSpace(service.Address) ? entry.Node.Address : service.Address;

            instances.Add(new ServiceInstance
            {
                InstanceId = service.ID,
                Address = address,
                Port = service.Port,
                Weight = weight
            });
        }

        return instances;
    }
}
