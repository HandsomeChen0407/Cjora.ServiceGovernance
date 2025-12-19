namespace Cjora.ServiceGovernance.Service;

/// <summary>
/// Consul 服务注册实现
/// 
/// 特性：
/// - TTL 心跳，避免 Consul 主动探测
/// - 支持 Weight=0 临时下线（发版）
/// </summary>
public sealed class ConsulServiceRegistry : IServiceRegistry
{
    private readonly IConsulClient _client;
    private readonly ServiceGovernanceOptions _options;
    private string _serviceId = default!;

    public ConsulServiceRegistry(
        IConsulClient client,
        IOptions<ServiceGovernanceOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task RegisterAsync(CancellationToken ct = default)
    {
        _serviceId = $"{_options.ServiceName}_{Guid.NewGuid()}";

        var registration = new AgentServiceRegistration
        {
            ID = _serviceId,
            Name = _options.ServiceName,
            Address = _options.ServiceAddress,
            Port = _options.ServicePort,
            Meta = new Dictionary<string, string>
            {
                ["Weight"] = _options.Weight.ToString()
            },
            Check = new AgentServiceCheck
            {
                TTL = TimeSpan.FromSeconds(30),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(5)
            }
        };

        await _client.Agent.ServiceRegister(registration, ct);

        _ = Task.Run(() => HeartbeatLoop(ct), ct);
    }

    /// <summary>
    /// 优雅下线：
    /// 1. 将 Weight 设置为 0
    /// 2. 等待流量耗尽
    /// 3. 注销
    /// </summary>
    public async Task DeregisterAsync(CancellationToken ct = default)
    {
        try
        {
            await _client.Agent.ServiceDeregister(_serviceId, ct);
        }
        catch
        {
            // 忽略异常，避免阻塞进程退出
        }
    }

    private async Task HeartbeatLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await _client.Agent.PassTTL(
                    $"service:{_serviceId}",
                    "OK",
                    ct);
            }
            catch { }

            await Task.Delay(TimeSpan.FromSeconds(20), ct);
        }
    }
}
