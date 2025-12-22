# Cjora.ServiceGovernance

**Cjora.ServiceGovernance** 是一个基于 **.NET / ASP.NET Core** 的轻量级服务治理框架，提供：

- 服务注册 / 服务发现
- 客户端负载均衡
- 配置中心（支持热更新）
- HttpClient 服务发现 + 重试 / 熔断
- 插件化扩展注册中心与配置中心（Consul / Nacos / Etcd）

框架默认实现基于 **Consul**，整体设计参考 **Spring Cloud** 的理念，但更贴合 **.NET 依赖注入、HttpClientFactory 与 Options 模型**。

---

## 功能特性

- **服务注册**
  - 应用启动自动注册
  - TTL 心跳机制
  - 支持优雅下线

- **服务发现**
  - 动态发现可用实例
  - 与负载均衡策略解耦

- **负载均衡**
  - 内置加权轮询算法
  - 支持扩展随机 / 最少连接等策略

- **HttpClient 扩展**
  - 使用服务名直接调用下游服务
  - 内置 Polly 重试 / 熔断
  - 对业务代码无侵入

- **配置中心**
  - 基于 Consul KV
  - 本地缓存 + Watch 长轮询
  - 支持配置删除事件

- **Options 热更新**
  - 配置中心 JSON 自动绑定 `IOptionsMonitor<T>`
  - 运行时自动刷新

- **配置加密**
  - 配置中心存储密文
  - 业务侧始终使用明文

---

## 安装

```bash
dotnet add package Cjora.ServiceGovernance
```

---

## 配置示例（appsettings.json）

```json
{
  "ServiceGovernance": {
    "RegistryType": "Consul",
    "RegistryAddress": "http://127.0.0.1:8500",

    "ServiceName": "order-service",
    "ServiceAddress": "127.0.0.1",
    "ServicePort": 5000,
    "Weight": 1,

    "Discovery": {
      "CacheSeconds": 10
    },

    "Resilience": {
      "RetryCount": 3,
      "CircuitBreakerFailures": 5,
      "CircuitBreakerSeconds": 30
    },

    "ConfigCenter": {
      "Enabled": true,
      "Type": "Consul"
    }
  }
}
```

---

## Program.cs

```csharp
using Cjora.ServiceGovernance.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 注册服务治理（注册中心 + 配置中心）
builder.Services.AddServiceGovernance(builder.Configuration);

// 注册支持服务发现 + 重试 / 熔断的 HttpClient
builder.Services.AddServiceDiscoveryHttpClient("default");

var app = builder.Build();
app.Run();
```

---

## 获取服务地址（服务发现 + 负载均衡）

```csharp
app.MapGet("/service-uri", async (IServiceGovernance governance) =>
{
    var uri = await governance.GetServiceUriAsync("user-service");
    return Results.Ok(uri.ToString());
});
```

---

## HttpClient 使用服务名直连

```csharp
public sealed class OrderService
{
    private readonly HttpClient _client;

    public OrderService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient("default");
    }

    public async Task<string> GetUserAsync()
    {
        return await _client.GetStringAsync(
            "http://user-service/api/users/1");
    }
}
```

---

## 读取配置中心配置

```csharp
app.MapGet("/config/get", async (IConfigCenter configCenter) =>
{
    var value = await configCenter.GetConfigAsync(
        key: "Redis:ConnectionString",
        group: "default");

    return Results.Ok(value);
});
```

---

## 写入 / 删除配置

```csharp
await configCenter.SetConfigAsync(
    key: "FeatureFlags:EnableNewUI",
    value: "true",
    group: "default");

await configCenter.DeleteConfigAsync(
    key: "FeatureFlags:EnableNewUI",
    group: "default");
```

---


## 监听配置变更

```csharp
var handle = await configCenter.WatchConfigAsync(
    key: "Redis:ConnectionString",
    onChanged: e =>
    {
        if (e.IsDeleted)
            Console.WriteLine($"配置被删除: {e.Key}");
        else
            Console.WriteLine($"配置更新: {e.Key} = {e.Value}");
    },
    group: "default");

// 不再需要监听时
handle.Dispose();
```

---

## 配置中心 + Options（热更新）

```csharp
builder.Services.AddConfigCenterOptions<RedisOptions>(
    key: "Redis",
    group: "default");

public sealed class DemoService
{
    public DemoService(IOptionsMonitor<RedisOptions> options)
    {
        options.OnChange(o =>
        {
            Console.WriteLine("Redis 配置更新：" + o.ConnectionString);
        });
    }
}

public sealed class RedisOptions
{
    public string ConnectionString { get; set; } = default!;
}

```