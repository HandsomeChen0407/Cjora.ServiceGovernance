namespace Cjora.ServiceGovernance.Extensions;

public static class ServiceGovernanceSetup
{
    private static readonly List<IRegistryModule> _registryModules = new();
    private static readonly List<IConfigCenterModule> _configCenterModules = new();

    public static IServiceCollection AddServiceGovernance(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ServiceGovernanceOptions>(
            configuration.GetSection("ServiceGovernance"));

        var options = configuration
            .GetSection("ServiceGovernance")
            .Get<ServiceGovernanceOptions>()
            ?? throw new InvalidOperationException("ServiceGovernance 配置缺失");

        // ========= 注册中心 =========
        var registryModule = _registryModules.FirstOrDefault(m =>
            m.RegistryType.Equals(options.RegistryType, StringComparison.OrdinalIgnoreCase));

        if (registryModule == null)
            throw new NotSupportedException(
                $"未注册注册中心模块：{options.RegistryType}");

        registryModule.Register(services, options);

        // ========= 配置中心 =========
        if (options.ConfigCenter.Enabled)
        {
            var configModule = _configCenterModules.FirstOrDefault(m =>
                m.Type.Equals(options.ConfigCenter.Type, StringComparison.OrdinalIgnoreCase));

            if (configModule == null)
                throw new NotSupportedException(
                    $"未注册配置中心模块：{options.ConfigCenter.Type}");

            configModule.Register(services, options);
        }

        // ========= 通用能力 =========
        services.AddHostedService<ServiceRegistryHostedService>();
        services.AddSingleton<ILoadBalancer, WeightedRoundRobinLoadBalancer>();
        services.AddSingleton<IServiceGovernance, ServiceGovernanceService>();

        return services;
    }

    // ===== 模块注册（框架内部） =====

    internal static void AddRegistryModule(IRegistryModule module)
        => _registryModules.Add(module);

    internal static void AddConfigCenterModule(IConfigCenterModule module)
        => _configCenterModules.Add(module);
}