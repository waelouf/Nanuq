namespace Nanuq.Security.Interfaces;

public interface ICredentialService
{
    /// <summary>
    /// Encrypts plaintext using AES-256 encryption
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <returns>Base64-encoded encrypted string with IV prepended</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts ciphertext that was encrypted with Encrypt method
    /// </summary>
    /// <param name="encryptedText">Base64-encoded encrypted string</param>
    /// <returns>Decrypted plaintext</returns>
    string Decrypt(string encryptedText);

    /// <summary>
    /// Gets the encryption key identifier for versioning
    /// </summary>
    /// <returns>Encryption key ID</returns>
    string GetEncryptionKeyId();
}
