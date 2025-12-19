namespace Cjora.ServiceGovernance.Extensions;

/// <summary>
/// Consul 服务治理注册扩展。
/// <para>
/// 负责注册 Consul 客户端、
/// 以及服务注册 + TTL 心跳后台任务。
/// </para>
/// </summary>
public static class ConsulSetup
{
    /// <summary>
    /// 添加 Consul 服务治理能力（服务注册 + TTL 心跳）。
    /// </summary>
    public static IServiceCollection AddConsul(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. 绑定配置
        services.Configure<ServiceGovernanceOptions>(
            configuration.GetSection("ServiceGovernance"));

        // 2. 注册 Consul 客户端（原生方式）
        services.AddSingleton<IConsulClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ServiceGovernanceOptions>>().Value;

            return new ConsulClient(cfg =>
            {
                cfg.Address = new Uri(options.RegistryAddress);
            });
        });

        // 3. 注册 TTL 心跳服务
        services.AddHostedService<ConsulTtlHostedService>();

        return services;
    }
}
