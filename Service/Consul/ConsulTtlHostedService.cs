namespace Cjora.ServiceGovernance.Service;

/// <summary>
/// Consul TTL 心跳后台服务
/// </summary>
public sealed class ConsulTtlHostedService : BackgroundService
{
    private readonly IConsulClient _client;
    private readonly ServiceGovernanceOptions _options;
    private readonly ILogger<ConsulTtlHostedService> _logger;

    private readonly string _serviceId;
    private readonly string _checkId;

    public ConsulTtlHostedService(
        IConsulClient client,
        IOptions<ServiceGovernanceOptions> options,
        ILogger<ConsulTtlHostedService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;

        _serviceId = $"{_options.ServiceName}-{Guid.NewGuid():N}";
        _checkId = $"ttl:{_serviceId}";
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await RegisterServiceAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _client.Agent.PassTTL(_checkId, "OK", stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Consul TTL heartbeat failed, try next round");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _client.Agent.ServiceDeregister(_serviceId, cancellationToken);
            _logger.LogInformation("Service deregistered from Consul: {ServiceId}", _serviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deregister service from Consul");
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task RegisterServiceAsync(CancellationToken ct)
    {
        var registration = new AgentServiceRegistration
        {
            ID = _serviceId,
            Name = _options.ServiceName,
            Address = _options.ServiceAddress,
            Port = _options.ServicePort,
            Meta = new Dictionary<string, string>
            {
                ["weight"] = _options.Weight.ToString()
            },
            Check = new AgentServiceCheck
            {
                CheckID = _checkId,
                TTL = TimeSpan.FromSeconds(30),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(2)
            }
        };

        await _client.Agent.ServiceRegister(registration, ct);

        _logger.LogInformation("Service registered to Consul. Name={ServiceName}, Id={ServiceId}",
            _options.ServiceName, _serviceId);
    }
}
