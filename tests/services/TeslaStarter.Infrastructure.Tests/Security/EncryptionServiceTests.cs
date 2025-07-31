using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TeslaStarter.Infrastructure.Security;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.Security;

public class EncryptionServiceTests
{
    [Fact]
    public void Constructor_Should_Use_Provided_Key()
    {
        // Arrange
        var configuration = CreateConfiguration("TestEncryptionKey123!");

        // Act
        var service = new EncryptionService(configuration);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_Should_Use_Placeholder_Key_When_Key_Is_Missing()
    {
        // Arrange
        var configuration = CreateConfiguration(null);

        // Act
        var service = new EncryptionService(configuration);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_Should_Use_Placeholder_Key_When_Key_Is_Empty()
    {
        // Arrange
        var configuration = CreateConfiguration("");

        // Act
        var service = new EncryptionService(configuration);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Encrypt_Should_Return_Null_For_Null_Input()
    {
        // Arrange
        var configuration = CreateConfiguration("TestKey");
        var service = new EncryptionService(configuration);

        // Act
        var result = service.Encrypt(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Encrypt_Should_Return_Empty_For_Empty_Input()
    {
        // Arrange
        var configuration = CreateConfiguration("TestKey");
        var service = new EncryptionService(configuration);

        // Act
        var result = service.Encrypt("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Encrypt_Should_Encrypt_Plain_Text()
    {
        // Arrange
        var configuration = CreateConfiguration("TestEncryptionKey123!");
        var service = new EncryptionService(configuration);
        var plainText = "This is a secret token";

        // Act
        var encrypted = service.Encrypt(plainText);

        // Assert
        encrypted.Should().NotBeNull();
        encrypted.Should().NotBe(plainText);
        encrypted.Should().NotBeEmpty();
        // Base64 encoded string should be properly formatted
        Convert.FromBase64String(encrypted!).Should().NotBeEmpty();
    }

    [Fact]
    public void Encrypt_Should_Produce_Different_Results_For_Same_Input()
    {
        // Arrange
        var configuration = CreateConfiguration("TestEncryptionKey123!");
        var service = new EncryptionService(configuration);
        var plainText = "This is a secret token";

        // Act
        var encrypted1 = service.Encrypt(plainText);
        var encrypted2 = service.Encrypt(plainText);

        // Assert
        encrypted1.Should().NotBe(encrypted2); // Different IVs should produce different results
    }

    [Fact]
    public void Decrypt_Should_Return_Null_For_Null_Input()
    {
        // Arrange
        var configuration = CreateConfiguration("TestKey");
        var service = new EncryptionService(configuration);

        // Act
        var result = service.Decrypt(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Decrypt_Should_Return_Empty_For_Empty_Input()
    {
        // Arrange
        var configuration = CreateConfiguration("TestKey");
        var service = new EncryptionService(configuration);

        // Act
        var result = service.Decrypt("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_Should_Decrypt_Encrypted_Text()
    {
        // Arrange
        var configuration = CreateConfiguration("TestEncryptionKey123!");
        var service = new EncryptionService(configuration);
        var plainText = "This is a secret token";
        var encrypted = service.Encrypt(plainText);

        // Act
        var decrypted = service.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void Encrypt_Decrypt_Should_Work_With_Various_Strings()
    {
        // Arrange
        var configuration = CreateConfiguration("TestEncryptionKey123!");
        var service = new EncryptionService(configuration);
        var testStrings = new[]
        {
            "Simple text",
            "Text with special characters: !@#$%^&*()",
            "Text with numbers: 1234567890",
            "Unicode text: 你好世界 🌍",
            "Very long text: " + new string('a', 1000),
            "Token-like string: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ"
        };

        foreach (var testString in testStrings)
        {
            // Act
            var encrypted = service.Encrypt(testString);
            var decrypted = service.Decrypt(encrypted);

            // Assert
            decrypted.Should().Be(testString);
        }
    }

    [Fact]
    public void Decrypt_Should_Throw_For_Invalid_Base64()
    {
        // Arrange
        var configuration = CreateConfiguration("TestKey");
        var service = new EncryptionService(configuration);

        // Act & Assert
        var action = () => service.Decrypt("Not a valid base64 string!");
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void Decrypt_Should_Throw_For_Invalid_Encrypted_Data()
    {
        // Arrange
        var configuration = CreateConfiguration("TestKey");
        var service = new EncryptionService(configuration);
        // Valid base64 but not valid encrypted data
        var invalidData = Convert.ToBase64String(Encoding.UTF8.GetBytes("Invalid encrypted data"));

        // Act & Assert
        var action = () => service.Decrypt(invalidData);
        action.Should().Throw<Exception>(); // Could be ArgumentException or CryptographicException
    }

    [Fact]
    public void GetKeyBytes_Should_Always_Return_32_Bytes()
    {
        // This tests the private method indirectly through encryption
        var testKeys = new[] { "short", "medium length key", "very long key that is much longer than 32 bytes" };

        foreach (var key in testKeys)
        {
            // Arrange
            var configuration = CreateConfiguration(key);
            var service = new EncryptionService(configuration);
            var plainText = "test";

            // Act - if key generation works correctly, encryption should succeed
            var encrypted = service.Encrypt(plainText);
            var decrypted = service.Decrypt(encrypted);

            // Assert
            decrypted.Should().Be(plainText);
        }
    }

    private static IConfiguration CreateConfiguration(string? encryptionKey)
    {
        var configValues = new Dictionary<string, string?>();
        if (encryptionKey != null)
        {
            configValues["Encryption:Key"] = encryptionKey;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
    }
}
