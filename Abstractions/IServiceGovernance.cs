namespace Cjora.ServiceGovernance.Abstractions;

/// <summary>
/// 服务治理统一接口
/// 业务层直接依赖此接口获取服务访问地址
/// </summary>
public interface IServiceGovernance
{
    /// <summary>
    /// 获取服务访问地址（自动负载均衡）
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>服务实例 URI</returns>
    Task<Uri> GetServiceUriAsync(string serviceName, CancellationToken cancellationToken = default);
}
