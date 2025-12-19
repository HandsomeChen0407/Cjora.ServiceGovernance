namespace Cjora.ServiceGovernance.Options;

/// <summary>
/// 服务治理统一配置
/// </summary>
public sealed class ServiceGovernanceOptions
{
    /// <summary>
    /// 服务注册类型，默认 Consul
    /// </summary>
    public string RegistryType { get; set; } = "Consul";

    /// <summary>
    /// 服务注册地址
    /// </summary>
    public string RegistryAddress { get; set; } = default!;

    /// <summary>
    /// 服务名
    /// </summary>
    public string ServiceName { get; set; } = default!;

    /// <summary>
    /// 实例地址（用于服务注册）
    /// </summary>
    public string ServiceAddress { get; set; } = default!;

    /// <summary>
    /// 实例端口
    /// </summary>
    public int ServicePort { get; set; }

    /// <summary>
    /// 权重
    /// </summary>
    public int Weight { get; set; } = 1;

    /// <summary>
    /// 服务发现相关配置
    /// </summary>
    public DiscoveryOptions Discovery { get; set; } = new();

    /// <summary>
    /// 服务熔断和重试策略配置
    /// </summary>
    public ResilienceOptions Resilience { get; set; } = new();
}

/// <summary>
/// 服务发现配置
/// </summary>
public sealed class DiscoveryOptions
{
    /// <summary>
    /// 服务发现缓存刷新间隔（秒）
    /// </summary>
    public int CacheSeconds { get; set; } = 10;
}

/// <summary>
/// 熔断重试配置
/// </summary>
public sealed class ResilienceOptions
{
    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// 熔断失败次数
    /// </summary>
    public int CircuitBreakerFailures { get; set; } = 5;

    /// <summary>
    /// 熔断持续秒数
    /// </summary>
    public int CircuitBreakerSeconds { get; set; } = 30;
}
