namespace Cjora.ServiceGovernance.Interfaces;

/// <summary>
/// 配置加密器抽象
/// 
/// 设计目标：
/// - 配置在配置中心中以密文形式存储
/// - 业务代码始终只接触明文
/// - 支持透明加解密
/// 
/// 使用场景：
/// - 数据库连接串
/// - 密钥 / Token
/// - 第三方账号凭证
/// </summary>
public interface IConfigEncryptor
{
    /// <summary>
    /// 判断指定值是否为加密内容
    /// 
    /// 例如：
    ///  - ENC(xxx)
    /// </summary>
    bool IsEncrypted(string value);

    /// <summary>
    /// 加密明文
    /// 
    /// 调用时机：
    ///  - 写入配置中心之前
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// 解密密文
    /// 
    /// 调用时机：
    ///  - 从配置中心读取之后
    /// </summary>
    string Decrypt(string cipherText);
}
