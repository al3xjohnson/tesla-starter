namespace TeslaStarter.Application.Common.Interfaces;

/// <summary>
/// Service for encrypting and decrypting sensitive data
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a plaintext string
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <returns>The encrypted text, or null if input is null</returns>
    string? Encrypt(string? plainText);

    /// <summary>
    /// Decrypts an encrypted string
    /// </summary>
    /// <param name="cipherText">The encrypted text to decrypt</param>
    /// <returns>The decrypted text, or null if input is null</returns>
    string? Decrypt(string? cipherText);
}
