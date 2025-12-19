# Cjora.ServiceGovernance

Cjora.ServiceGovernance 是一个基于 .NET 的服务治理框架，支持 **服务注册、发现、负载均衡、配置中心** 和 **HttpClient 服务调用的重试/熔断策略**。默认实现基于 **Consul**，可扩展支持 Nacos 等注册中心。

---

## 功能

- **服务注册**
- **服务发现**
- **负载均衡**
- **配置中心**
- **HttpClient 扩展（重试/熔断策略）**

---

## 安装

```bash
dotnet add package Cjora.ServiceGovernance
```
---

## 配置实例

```json
{
  "ServiceGovernance": {
    "RegistryType": "Consul",
    "RegistryAddress": "http://127.0.0.1:8500",
    "ServiceName": "MyService",
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
    }
  }
}
```

---

## Program.cs 示例

```csharp
using Cjora.ServiceGovernance.Extensions;
using Cjora.ServiceGovernance.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceGovernance(builder.Configuration);
builder.Services.AddServiceDiscoveryHttpClient("MyClient");

var app = builder.Build();

app.MapGet("/service-uri", async (IServiceGovernance governance) =>
{
    var uri = await governance.GetServiceUriAsync("AnotherService");
    return Results.Ok(uri.ToString());
});

app.MapGet("/config", async (IConfigCenter configCenter) =>
{
    string? value = await configCenter.GetConfigAsync("MyKey");
    await configCenter.WatchConfigAsync("MyKey", newValue =>
    {
        Console.WriteLine($"配置更新: {newValue}");
    });
    return Results.Ok(value);
});

app.MapGet("/load-balance", async (IServiceDiscovery discovery, ILoadBalancer loadBalancer) =>
{
    var instances = await discovery.GetInstancesAsync("AnotherService");
    var selected = loadBalancer.Select(instances, "AnotherService");
    return Results.Ok(selected.ToUri().ToString());
});

app.Run();
```
---

## 读取配置

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

## 写入配置

```csharp

await configCenter.SetConfigAsync(
    key: "FeatureFlags:EnableNewUI",
    value: "true",
    group: "default");

```
---

## 删除配置

```csharp

await configCenter.DeleteConfigAsync(
    key: "FeatureFlags:EnableNewUI",
    group: "default");

```
---

## 配置监听

```csharp

var handle = await configCenter.WatchConfigAsync(
    key: "Redis:ConnectionString",
    onChanged: e =>
    {
        if (e.IsDeleted)
        {
            Console.WriteLine($"配置被删除: {e.Key}");
        }
        else
        {
            Console.WriteLine($"配置更新: {e.Key} = {e.Value}");
        }
    },
    group: "default");

// 应用关闭或不再需要时取消监听
handle.Dispose();

```
---

## 配置中心 Program.cs 实例

```csharp

var builder = WebApplication.CreateBuilder(args);

// 注册配置中心（Consul）
builder.Services.AddSingleton<IConfigCenter, ConsulConfigCenter>();

// 注册 ConfigCenter Options（就在这里）
builder.Services.AddConfigCenterOptions<RedisOptions>(
    key: "Redis",
    group: "default");

var app = builder.Build();
app.Run();

```