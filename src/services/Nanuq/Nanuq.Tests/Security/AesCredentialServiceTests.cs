using FluentAssertions;
using Nanuq.Common.Exceptions;
using Nanuq.Security.Services;
using Xunit;

namespace Nanuq.Tests.Security;

public class AesCredentialServiceTests
{
    #region Encryption Tests

    [Fact]
    public void Encrypt_ReturnsBase64String_WhenValidPlainText()
    {
        // Arrange
        var service = new AesCredentialService();
        var plainText = "test password";

        // Act
        var result = service.Encrypt(plainText);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "Base64 string should only contain valid Base64 characters");

        // Verify it's valid Base64 by attempting to decode
        var action = () => Convert.FromBase64String(result);
        action.Should().NotThrow();
    }

    [Fact]
    public void Encrypt_ThrowsArgumentNullException_WhenPlainTextIsNull()
    {
        // Arrange
        var service = new AesCredentialService();
        string? plainText = null;

        // Act
        var action = () => service.Encrypt(plainText!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("plainText");
    }

    [Fact]
    public void Encrypt_ThrowsArgumentNullException_WhenPlainTextIsEmpty()
    {
        // Arrange
        var service = new AesCredentialService();
        var plainText = string.Empty;

        // Act
        var action = () => service.Encrypt(plainText);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("plainText");
    }

    [Fact]
    public void Encrypt_GeneratesDifferentCiphertext_ForSameInputTwice()
    {
        // Arrange
        var service = new AesCredentialService();
        var plainText = "test password";

        // Act
        var result1 = service.Encrypt(plainText);
        var result2 = service.Encrypt(plainText);

        // Assert
        result1.Should().NotBe(result2, "Each encryption should use a different random IV");
    }

    [Fact]
    public void Encrypt_HandlesUnicodeCharacters_Correctly()
    {
        // Arrange
        var service = new AesCredentialService();
        var plainText = "こんにちは世界 🌍 Español ñ";

        // Act
        var result = service.Encrypt(plainText);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var decrypted = service.Decrypt(result);
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void Encrypt_HandlesLongStrings_Correctly()
    {
        // Arrange
        var service = new AesCredentialService();
        var plainText = new string('A', 10000); // 10KB string

        // Act
        var result = service.Encrypt(plainText);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var decrypted = service.Decrypt(result);
        decrypted.Should().Be(plainText);
    }

    #endregion

    #region Decryption Tests

    [Fact]
    public void Decrypt_ReturnsOriginalPlainText_WhenValidEncryptedText()
    {
        // Arrange
        var service = new AesCredentialService();
        var plainText = "test password";
        var encrypted = service.Encrypt(plainText);

        // Act
        var result = service.Decrypt(encrypted);

        // Assert
        result.Should().Be(plainText);
    }

    [Fact]
    public void Decrypt_ThrowsArgumentNullException_WhenEncryptedTextIsNull()
    {
        // Arrange
        var service = new AesCredentialService();
        string? encryptedText = null;

        // Act
        var action = () => service.Decrypt(encryptedText!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("encryptedText");
    }

    [Fact]
    public void Decrypt_ThrowsArgumentNullException_WhenEncryptedTextIsEmpty()
    {
        // Arrange
        var service = new AesCredentialService();
        var encryptedText = string.Empty;

        // Act
        var action = () => service.Decrypt(encryptedText);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("encryptedText");
    }

    [Fact]
    public void Decrypt_ThrowsEncryptionException_WhenInvalidBase64Format()
    {
        // Arrange
        var service = new AesCredentialService();
        var invalidBase64 = "This is not valid Base64!!!";

        // Act
        var action = () => service.Decrypt(invalidBase64);

        // Assert
        action.Should().Throw<EncryptionException>()
            .WithMessage("Invalid encrypted text format*");
    }

    [Fact]
    public void Decrypt_ThrowsEncryptionException_WhenCorruptedCiphertext()
    {
        // Arrange
        var service = new AesCredentialService();
        var plainText = "test password";
        var encrypted = service.Encrypt(plainText);

        // Corrupt the encrypted data (flip some bits in the middle)
        var bytes = Convert.FromBase64String(encrypted);
        bytes[bytes.Length / 2] ^= 0xFF; // Flip bits
        var corruptedEncrypted = Convert.ToBase64String(bytes);

        // Act
        var action = () => service.Decrypt(corruptedEncrypted);

        // Assert
        action.Should().Throw<EncryptionException>()
            .WithMessage("Failed to decrypt data*");
    }

    [Fact]
    public void Decrypt_ThrowsEncryptionException_WhenCiphertextTooShort()
    {
        // Arrange
        var service = new AesCredentialService();
        // Create a Base64 string that's too short to contain a valid IV (16 bytes minimum)
        var tooShort = Convert.ToBase64String(new byte[8]);

        // Act
        var action = () => service.Decrypt(tooShort);

        // Assert
        action.Should().Throw<EncryptionException>();
    }

    #endregion

    #region Round-trip Tests

    [Fact]
    public void EncryptDecrypt_ReturnsOriginalText_WhenValidInput()
    {
        // Arrange
        var service = new AesCredentialService();
        var plainText = "MySecretPassword123!@#";

        // Act
        var encrypted = service.Encrypt(plainText);
        var decrypted = service.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void EncryptDecrypt_HandlesUnicode_Correctly()
    {
        // Arrange
        var service = new AesCredentialService();
        var plainText = "Пароль 密码 كلمة السر 😀🔐";

        // Act
        var encrypted = service.Encrypt(plainText);
        var decrypted = service.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void EncryptDecrypt_HandlesLongStrings_Correctly()
    {
        // Arrange
        var service = new AesCredentialService();
        var plainText = new string('X', 5000);

        // Act
        var encrypted = service.Encrypt(plainText);
        var decrypted = service.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void EncryptDecrypt_HandlesJsonPayloads_Correctly()
    {
        // Arrange
        var service = new AesCredentialService();
        var jsonPayload = @"{""username"":""admin"",""password"":""secret123"",""apiKey"":""xyz789""}";

        // Act
        var encrypted = service.Encrypt(jsonPayload);
        var decrypted = service.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(jsonPayload);
    }

    #endregion

    #region Key Management Tests

    [Fact]
    public void GetEncryptionKeyId_ReturnsConsistent8CharacterId()
    {
        // Arrange
        var service = new AesCredentialService();

        // Act
        var keyId = service.GetEncryptionKeyId();

        // Assert
        keyId.Should().NotBeNullOrEmpty();
        keyId.Should().HaveLength(8, "Key ID should be 8 characters (first 8 of Base64 hash)");
        keyId.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "Key ID should be Base64");
    }

    [Fact]
    public void Constructor_InitializesKeySuccessfully()
    {
        // Arrange & Act
        var service = new AesCredentialService();
        var keyId = service.GetEncryptionKeyId();

        // Assert
        keyId.Should().NotBeNullOrEmpty();

        // Verify the service is functional
        var testText = "test";
        var encrypted = service.Encrypt(testText);
        var decrypted = service.Decrypt(encrypted);
        decrypted.Should().Be(testText);
    }

    [Fact]
    public void DeriveEncryptionKey_CreatesValidKeyId()
    {
        // Arrange & Act
        var service = new AesCredentialService();
        var keyId = service.GetEncryptionKeyId();

        // Assert
        keyId.Should().NotBeNullOrEmpty();
        keyId.Should().HaveLength(8);
        keyId.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "Key ID should be valid Base64");

        // Verify the service remains functional after getting key ID
        var testText = "test";
        var encrypted = service.Encrypt(testText);
        var decrypted = service.Decrypt(encrypted);
        decrypted.Should().Be(testText);
    }

    #endregion
}
