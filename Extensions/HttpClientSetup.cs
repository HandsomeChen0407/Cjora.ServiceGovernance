namespace Cjora.ServiceGovernance.Extensions;

/// <summary>
/// HttpClientFactory 扩展
/// 自动支持服务发现 + Polly 重试熔断
/// </summary>
public static class HttpClientSetup
{
    public static IHttpClientBuilder AddServiceDiscoveryHttpClient(this IServiceCollection services, string name)
    {
        return services.AddHttpClient(name)
            .AddHttpMessageHandler(sp =>
            {
                var governance = sp.GetRequiredService<IServiceGovernance>();
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Cjora.ServiceGovernance.Options.ServiceGovernanceOptions>>().Value;
                return new ServiceDiscoveryHttpHandler(governance,
                    options.Resilience.RetryCount,
                    options.Resilience.CircuitBreakerFailures,
                    options.Resilience.CircuitBreakerSeconds);
            });
    }
}
