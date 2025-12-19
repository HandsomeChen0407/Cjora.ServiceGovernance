using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Cjora.ServiceGovernance.Abstractions;

namespace Cjora.ServiceGovernance.Options;

/// <summary>
/// ConfigCenter 与 IOptions 的集成扩展
/// 
/// 功能：
/// - 将配置中心中的 JSON 配置
/// - 自动绑定为 IOptionsMonitor&lt;T&gt;
/// - 支持运行时热更新
/// </summary>
public static class ConfigCenterOptionsExtensions
{
    /// <summary>
    /// 注册基于配置中心的 Options
    /// 
    /// 使用示例：
    /// <code>
    /// services.AddConfigCenterOptions&lt;RedisOptions&gt;(
    ///     key: "redis",
    ///     group: "infrastructure");
    /// </code>
    /// </summary>
    public static IServiceCollection AddConfigCenterOptions<T>(
        this IServiceCollection services,
        string key,
        string? group = null)
        where T : class, new()
    {
        services.AddSingleton<IOptionsMonitor<T>>(sp =>
        {
            var center = sp.GetRequiredService<IConfigCenter>();
            return new ConfigCenterOptionsMonitor<T>(center, key, group);
        });

        return services;
    }
}
