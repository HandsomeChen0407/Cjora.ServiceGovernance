namespace Cjora.ServiceGovernance.Service;

/// <summary>
/// Consul TTL 心跳后台服务。
/// </summary>
public sealed class ConsulTtlHeartbeatService : BackgroundService
{
    private readonly IConsulClient _client;
    private readonly ServiceGovernanceOptions _options;
    private readonly ILogger<ConsulTtlHeartbeatService> _logger;

    public ConsulTtlHeartbeatService(
        IConsulClient client,
        IOptions<ServiceGovernanceOptions> options,
        ILogger<ConsulTtlHeartbeatService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var checkId = $"service:{_options.ServiceName}";

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _client.Agent.PassTTL(checkId, "OK", stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Consul TTL heartbeat failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
