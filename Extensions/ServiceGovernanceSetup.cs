namespace Cjora.ServiceGovernance.Extensions;

/// <summary>
/// 服务治理依赖注入扩展
/// </summary>
public static class ServiceGovernanceSetup
{
    public static IServiceCollection AddServiceGovernance(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceGovernanceOptions>(configuration.GetSection("ServiceGovernance"));

        var options = configuration.GetSection("ServiceGovernance").Get<ServiceGovernanceOptions>()!;

        if (options.RegistryType.Equals("Consul", StringComparison.OrdinalIgnoreCase))
        {
            // 注册 Consul 客户端
            services.AddSingleton<IConsulClient>(_ => new ConsulClient(c => c.Address = new Uri(options.RegistryAddress)));

            // 注册服务注册和发现
            services.AddSingleton<IServiceRegistry, ConsulServiceRegistry>();
            services.AddSingleton<IServiceDiscovery, ConsulServiceDiscovery>();
        }
        else
        {
            throw new NotSupportedException("当前只支持 Consul，实现 Nacos 可以扩展");
        }

        // 缓存服务发现
        services.AddSingleton<IServiceDiscovery>(sp =>
            new CachedServiceDiscovery(sp.GetRequiredService<IServiceDiscovery>(), sp.GetRequiredService<IOptions<ServiceGovernanceOptions>>()));

        // 负载均衡
        services.AddSingleton<ILoadBalancer, WeightedRoundRobinLoadBalancer>();

        // 服务治理统一入口
        services.AddSingleton<IServiceGovernance, ServiceGovernanceService>();

        return services;
    }
}
