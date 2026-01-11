using System.Security.Cryptography;
using System.Text;
using Nanuq.Common.Exceptions;
using Nanuq.Security.Interfaces;

namespace Nanuq.Security.Services;

public class AesCredentialService : ICredentialService
{
    private readonly byte[] _key;
    private readonly string _keyId;

    public AesCredentialService()
    {
        // Generate a machine-specific key using DPAPI
        _key = DeriveEncryptionKey();
        _keyId = ComputeKeyId(_key);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            throw new ArgumentNullException(nameof(plainText));
        }

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV(); // Generate random IV for this encryption

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Prepend IV to ciphertext
            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }
        catch (Exception ex)
        {
            throw new EncryptionException("Failed to encrypt data", ex);
        }
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
        {
            throw new ArgumentNullException(nameof(encryptedText));
        }

        try
        {
            var fullCipher = Convert.FromBase64String(encryptedText);

            using var aes = Aes.Create();
            aes.Key = _key;

            // Extract IV from the beginning of the ciphertext
            var iv = new byte[aes.IV.Length];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (FormatException ex)
        {
            throw new EncryptionException("Invalid encrypted text format", ex);
        }
        catch (CryptographicException ex)
        {
            throw new EncryptionException("Failed to decrypt data", ex);
        }
        catch (Exception ex)
        {
            throw new EncryptionException("Unexpected error during decryption", ex);
        }
    }

    public string GetEncryptionKeyId()
    {
        return _keyId;
    }

    private byte[] DeriveEncryptionKey()
    {
        // For Windows: Use DPAPI to derive a machine-specific key
        // For other platforms: Use a combination of machine name and environment
        if (OperatingSystem.IsWindows())
        {
            // Use DPAPI to protect a known entropy value
            var entropy = Encoding.UTF8.GetBytes("Nanuq-Credential-Encryption-v1");
            var protectedEntropy = ProtectedData.Protect(entropy, null, DataProtectionScope.LocalMachine);

            // Derive a 32-byte (256-bit) key from the protected data
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(protectedEntropy);
        }
        else
        {
            // For non-Windows platforms, use machine-specific information
            // This is less secure but provides basic protection for dev/staging
            var machineInfo = $"{Environment.MachineName}-Nanuq-v1-{Environment.UserName}";
            var machineBytes = Encoding.UTF8.GetBytes(machineInfo);

            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(machineBytes);
        }
    }

    private string ComputeKeyId(byte[] key)
    {
        // Create a short identifier for the key (first 8 characters of hash)
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(key);
        return Convert.ToBase64String(hash)[..8];
    }
}
