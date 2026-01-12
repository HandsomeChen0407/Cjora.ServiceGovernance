using Cjora.ServiceGovernance.Extensions;
using Cjora.ServiceGovernance.Security;

namespace Cjora.ServiceGovernance.Service;

public sealed class ConsulConfigCenterModule : IConfigCenterModule
{
    /// <summary>
    /// 配置中心类型标识
    /// 必须与配置文件中的 ConfigCenter:Type 对应
    /// </summary>
    public string Type => "Consul";

    /// <summary>
    /// 注册 Consul 配置中心相关依赖
    /// </summary>
    public void Register(IServiceCollection services, ServiceGovernanceOptions options)
    {
        services.AddSingleton<IConfigCenter, ConsulConfigCenter>();

        // 加密
        services.AddSingleton<IConfigEncryptor>(
                new AesConfigEncryptor("cjh199547"));
    }

    /// <summary>
    /// ⭐ 关键：静态构造函数
    /// </summary>
    static ConsulConfigCenterModule()
    {
        ServiceGovernanceSetup.AddConfigCenterModule(
            new ConsulConfigCenterModule());
    }
}
