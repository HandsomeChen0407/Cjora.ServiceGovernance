namespace Cjora.ServiceGovernance.Abstractions;

/// <summary>
/// 注册中心模块（插件化扩展点）
/// </summary>
public interface IRegistryModule
{
    /// <summary>
    /// 注册中心类型（Consul / Nacos / Etcd）
    /// </summary>
    string RegistryType { get; }

    /// <summary>
    /// 注册该注册中心所需的服务
    /// </summary>
    void Register(
        IServiceCollection services,
        ServiceGovernanceOptions options);
}
