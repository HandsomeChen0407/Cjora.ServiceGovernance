using System.Security.Cryptography;

namespace Cjora.ServiceGovernance.Security;

/// <summary>
/// 基于 AES 的配置加密器
/// 
/// 密文格式：
/// ENC(base64)
/// 
/// 设计说明：
/// - 使用 SHA256 派生固定长度密钥
/// - IV 取前 16 字节
/// - 适用于配置存储，不适合大数据加密
/// </summary>
public sealed class AesConfigEncryptor : IConfigEncryptor
{
    private const string Prefix = "ENC(";
    private const string Suffix = ")";

    private readonly byte[] _key;
    private readonly byte[] _iv;

    /// <summary>
    /// 使用字符串密钥初始化加密器
    /// </summary>
    /// <param name="key">加密密钥（建议来自安全配置）</param>
    public AesConfigEncryptor(string key)
    {
        using var sha = SHA256.Create();
        _key = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
        _iv = _key[..16];
    }

    public bool IsEncrypted(string value)
        => value.StartsWith(Prefix) && value.EndsWith(Suffix);

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor();
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var cipher = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

        return $"{Prefix}{Convert.ToBase64String(cipher)}{Suffix}";
    }

    public string Decrypt(string cipherText)
    {
        var base64 = cipherText[Prefix.Length..^Suffix.Length];
        var cipher = Convert.FromBase64String(base64);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor();
        var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plain);
    }
}
