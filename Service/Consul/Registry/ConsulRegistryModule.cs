using Cjora.ServiceGovernance.Extensions;

public sealed class ConsulRegistryModule : IRegistryModule
{
    public string RegistryType => "Consul";

    public void Register(IServiceCollection services, ServiceGovernanceOptions options)
    {
        services.AddSingleton<IConsulClient>(_ =>
            new ConsulClient(cfg =>
            {
                cfg.Address = new Uri(options.RegistryAddress);
            }));
        services.AddSingleton<IServiceRegistry, ConsulServiceRegistry>();
        services.AddSingleton<IServiceDiscovery, ConsulServiceDiscovery>();
    }

    // ⭐ 关键：静态构造函数
    static ConsulRegistryModule()
    {
        ServiceGovernanceSetup.AddRegistryModule(new ConsulRegistryModule());
    }
}
