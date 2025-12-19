namespace Cjora.ServiceGovernance.Models;

/// <summary>
/// 服务实例模型
/// </summary>
public sealed class ServiceInstance
{
    /// <summary>
    /// 服务实例唯一ID
    /// </summary>
    public string InstanceId { get; init; } = default!;

    /// <summary>
    /// 服务 IP 地址
    /// </summary>
    public string Address { get; init; } = default!;

    /// <summary>
    /// 服务端口
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// 权重（用于加权负载均衡）
    /// </summary>
    public int Weight { get; init; } = 1;

    /// <summary>
    /// 返回 HTTP 基地址
    /// </summary>
    public Uri ToUri() => new($"http://{Address}:{Port}");
}
