namespace Cjora.ServiceGovernance.Service;

/// <summary>
/// Consul 服务注册实现
/// </summary>
public sealed class ConsulServiceRegistry : IServiceRegistry
{
    private readonly IConsulClient _client;
    private readonly ServiceGovernanceOptions _options;
    private readonly IHostApplicationLifetime _lifetime;
    private string _serviceId = default!;

    public ConsulServiceRegistry(IConsulClient client, IOptions<ServiceGovernanceOptions> options, IHostApplicationLifetime lifetime)
    {
        _client = client;
        _options = options.Value;
        _lifetime = lifetime;
        _serviceId = $"{_options.ServiceName}_{Guid.NewGuid()}";

        _lifetime.ApplicationStopping.Register(async () => await DeregisterAsync());
    }

    public async Task RegisterAsync(CancellationToken cancellationToken = default)
    {
        var registration = new AgentServiceRegistration
        {
            ID = _serviceId,
            Name = _options.ServiceName,
            Address = _options.ServiceAddress,
            Port = _options.ServicePort,
            Meta = new Dictionary<string, string> { { "Weight", _options.Weight.ToString() } },
            Check = new AgentServiceCheck
            {
                CheckID = $"service:{_serviceId}",
                TTL = TimeSpan.FromSeconds(30),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(5)
            }
        };

        await _client.Agent.ServiceRegister(registration, cancellationToken);

        _ = Task.Run(() => HeartbeatLoop(cancellationToken), cancellationToken);
    }

    private async Task HeartbeatLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _client.Agent.PassTTL($"service:{_serviceId}", "OK", cancellationToken);
            }
            catch
            {
                // 忽略异常
            }

            await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
        }
    }

    public async Task DeregisterAsync(CancellationToken cancellationToken = default)
    {
        await _client.Agent.ServiceDeregister(_serviceId, cancellationToken);
    }
}
