namespace Cjora.ServiceGovernance.Service;

/// <summary>
/// 服务注册后台任务
/// 
/// 启动：
/// 1. 注册服务
/// 
/// 停止（发版 / 重启）：
/// 1. 设置 Weight=0（从发现中摘除）
/// 2. 等待流量自然耗尽
/// 3. 注销服务
/// </summary>
public sealed class ServiceRegistryHostedService : IHostedService
{
    private readonly IServiceRegistry _registry;

    public ServiceRegistryHostedService(IServiceRegistry registry)
    {
        _registry = registry;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _registry.RegisterAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _registry.DeregisterAsync(cancellationToken);
    }
}
