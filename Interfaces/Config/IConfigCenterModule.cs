namespace Cjora.ServiceGovernance.Interfaces;

/// <summary>
/// 配置中心模块抽象
/// 用于屏蔽 Consul / Nacos / Etcd 差异
/// </summary>
public interface IConfigCenterModule
{
    /// <summary>
    /// 配置中心类型标识（Consul / Nacos / Etcd）
    /// </summary>
    string Type { get; }

    /// <summary>
    /// 注册配置中心相关依赖
    /// </summary>
    void Register(IServiceCollection services, ServiceGovernanceOptions options);
}
