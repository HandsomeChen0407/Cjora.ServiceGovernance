namespace Cjora.ServiceGovernance.Abstractions;

/// <summary>
/// 服务治理统一接口
/// 业务层直接调用该接口获取服务地址，无需关注底层注册中心
/// </summary>
public interface IServiceGovernance
{
    /// <summary>
    /// 获取服务访问地址（自动负载均衡）
    /// </summary>
    Task<Uri> GetServiceUriAsync(string serviceName, CancellationToken cancellationToken = default);
}
