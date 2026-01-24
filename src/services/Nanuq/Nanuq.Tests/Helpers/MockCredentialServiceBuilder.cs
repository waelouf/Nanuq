using Moq;
using Nanuq.Security.Interfaces;

namespace Nanuq.Tests.Helpers;

/// <summary>
/// Fluent builder for creating ICredentialService mocks with predefined behaviors
/// </summary>
public class MockCredentialServiceBuilder
{
    private readonly Mock<ICredentialService> _mock;
    private Func<string, string>? _encryptBehavior;
    private Func<string, string>? _decryptBehavior;
    private string _keyId = "TEST1234";

    public MockCredentialServiceBuilder()
    {
        _mock = new Mock<ICredentialService>();
    }

    /// <summary>
    /// Sets up predictable encryption: "text" → "encrypted_text"
    /// </summary>
    public MockCredentialServiceBuilder WithPredictableEncryption()
    {
        _encryptBehavior = text => $"encrypted_{text}";
        _decryptBehavior = text => text.Replace("encrypted_", "");
        return this;
    }

    /// <summary>
    /// Sets up custom encryption behavior
    /// </summary>
    public MockCredentialServiceBuilder WithEncryptBehavior(Func<string, string> encryptFunc, Func<string, string> decryptFunc)
    {
        _encryptBehavior = encryptFunc;
        _decryptBehavior = decryptFunc;
        return this;
    }

    /// <summary>
    /// Sets the encryption key ID
    /// </summary>
    public MockCredentialServiceBuilder WithKeyId(string keyId)
    {
        _keyId = keyId;
        return this;
    }

    /// <summary>
    /// Builds the configured mock
    /// </summary>
    public Mock<ICredentialService> Build()
    {
        if (_encryptBehavior != null)
        {
            _mock.Setup(x => x.Encrypt(It.IsAny<string>()))
                .Returns<string>(s => _encryptBehavior(s));
        }

        if (_decryptBehavior != null)
        {
            _mock.Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns<string>(s => _decryptBehavior(s));
        }

        _mock.Setup(x => x.GetEncryptionKeyId())
            .Returns(_keyId);

        return _mock;
    }
}
