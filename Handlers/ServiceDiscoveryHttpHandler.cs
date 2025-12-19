using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Cjora.ServiceGovernance.Handlers;

/// <summary>
/// HttpClientHandler 支持服务发现 + 重试熔断
/// </summary>
public sealed class ServiceDiscoveryHttpHandler : DelegatingHandler
{
    private readonly IServiceGovernance _governance;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

    public ServiceDiscoveryHttpHandler(
        IServiceGovernance governance,
        int retryCount = 3,
        int circuitBreakerFailures = 5,
        int circuitBreakerSeconds = 30)
    {
        _governance = governance;

        _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .RetryAsync(retryCount);

        _circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(circuitBreakerFailures, TimeSpan.FromSeconds(circuitBreakerSeconds));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var serviceName = request.RequestUri!.Host;
        var baseUri = await _governance.GetServiceUriAsync(serviceName, cancellationToken);

        // 替换请求 URL
        var builder = new UriBuilder(request.RequestUri!)
        {
            Host = baseUri.Host,
            Port = baseUri.Port
        };
        request.RequestUri = builder.Uri;

        return await _retryPolicy.ExecuteAsync(() =>
            _circuitBreakerPolicy.ExecuteAsync(() =>
                base.SendAsync(request, cancellationToken)
            )
        );
    }
}
