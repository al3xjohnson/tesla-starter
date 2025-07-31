using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using TeslaStarter.Application.Common.Interfaces;

namespace TeslaStarter.Infrastructure.Security;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private const string EncryptionKeyName = "Encryption:Key";

    public EncryptionService(IConfiguration configuration)
    {
        var keyString = configuration[EncryptionKeyName];

        // For design-time scenarios (migrations), use a placeholder key
        if (string.IsNullOrEmpty(keyString))
        {
            keyString = "DESIGN_TIME_PLACEHOLDER_KEY_DO_NOT_USE_IN_PRODUCTION";
        }

        // Ensure key is exactly 32 bytes (256 bits) for AES-256
        _key = GetKeyBytes(keyString);
    }

    public string? Encrypt(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Combine IV and cipher text
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
        Array.Copy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string? Decrypt(string? cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var buffer = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;

        // Extract IV from the beginning of the buffer
        var iv = new byte[aes.IV.Length];
        Array.Copy(buffer, 0, iv, 0, iv.Length);
        aes.IV = iv;

        // Extract cipher text
        var cipherBytes = new byte[buffer.Length - iv.Length];
        Array.Copy(buffer, iv.Length, cipherBytes, 0, cipherBytes.Length);

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    private static byte[] GetKeyBytes(string key)
    {
        // Use SHA256 to ensure we get exactly 32 bytes from any input
        return SHA256.HashData(Encoding.UTF8.GetBytes(key));
    }
}
