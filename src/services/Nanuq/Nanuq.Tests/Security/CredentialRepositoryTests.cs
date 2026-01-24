using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Nanuq.Common.Enums;
using Nanuq.Common.Exceptions;
using Nanuq.Common.Records;
using Nanuq.EF;
using Nanuq.Security.Interfaces;
using Nanuq.Sqlite.Repositories;
using Nanuq.Tests.Helpers;
using Xunit;

namespace Nanuq.Tests.Security;

public class CredentialRepositoryTests : IDisposable
{
    private readonly NanuqContext _context;
    private readonly CredentialRepository _repository;
    private readonly Mock<ICredentialService> _mockCredentialService;
    private readonly Mock<ILogger<CredentialRepository>> _mockLogger;

    public CredentialRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<NanuqContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new NanuqContext(options);
        _mockLogger = new Mock<ILogger<CredentialRepository>>();

        // Setup predictable encryption/decryption using builder
        _mockCredentialService = new MockCredentialServiceBuilder()
            .WithPredictableEncryption()
            .WithKeyId("TEST1234")
            .Build();

        _repository = new CredentialRepository(_mockLogger.Object, _context, _mockCredentialService.Object);
    }

    #region GetByServerAsync Tests

    [Fact]
    public async Task GetByServerAsync_ReturnsDecryptedCredential_WhenExists()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential(
            id: 1,
            serverId: 100,
            serverType: ServerType.Kafka,
            username: "encrypted_testuser",
            password: "encrypted_testpass"
        );
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByServerAsync(100, ServerType.Kafka);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser", "Username should be decrypted");
        result.Password.Should().Be("testpass", "Password should be decrypted");

        // Verify decrypt was called
        _mockCredentialService.Verify(x => x.Decrypt(It.IsAny<string>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task GetByServerAsync_ReturnsNull_WhenNotFound()
    {
        // Act
        var result = await _repository.GetByServerAsync(999, ServerType.Redis);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByServerAsync_ReturnsNull_WhenDecryptionFails()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential(
            id: 1,
            serverId: 100,
            serverType: ServerType.Kafka,
            username: "encrypted_data",
            password: "encrypted_data"
        );
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        // Setup decryption to throw exception
        _mockCredentialService
            .Setup(x => x.Decrypt(It.IsAny<string>()))
            .Throws(new EncryptionException("Decryption failed"));

        // Act
        var result = await _repository.GetByServerAsync(100, ServerType.Kafka);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByServerAsync_LogsError_WhenDecryptionFails()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential(
            id: 1,
            serverId: 100,
            serverType: ServerType.Kafka,
            username: "encrypted_data"
        );
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        _mockCredentialService
            .Setup(x => x.Decrypt(It.IsAny<string>()))
            .Throws(new EncryptionException("Decryption failed"));

        // Act
        await _repository.GetByServerAsync(100, ServerType.Kafka);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to decrypt credential")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByServerAsync_DecryptsUsername_WhenNotNull()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential(
            serverId: 100,
            username: "encrypted_admin",
            password: null
        );
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByServerAsync(100, ServerType.Kafka);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("admin");
        result.Password.Should().BeNull();
    }

    [Fact]
    public async Task GetByServerAsync_PreservesNullFields_WhenFieldsAreNull()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential(
            serverId: 100,
            username: null,
            password: null,
            additionalConfig: null
        );
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByServerAsync(100, ServerType.Kafka);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().BeNull();
        result.Password.Should().BeNull();
        result.AdditionalConfig.Should().BeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsDecryptedCredential_WhenExists()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential(
            id: 1,
            username: "encrypted_user",
            password: "encrypted_pass",
            additionalConfig: "encrypted_config"
        );
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("user");
        result.Password.Should().Be("pass");
        result.AdditionalConfig.Should().Be("config");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_DecryptsAllFields_Correctly()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential(
            id: 1,
            username: "encrypted_admin",
            password: "encrypted_secret123",
            additionalConfig: "encrypted_{\"key\":\"value\"}"
        );
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("admin");
        result.Password.Should().Be("secret123");
        result.AdditionalConfig.Should().Be("{\"key\":\"value\"}");

        // Verify decrypt called 3 times (username, password, additionalConfig)
        _mockCredentialService.Verify(x => x.Decrypt(It.IsAny<string>()), Times.Exactly(3));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenExceptionOccurs()
    {
        // Arrange
        _mockCredentialService
            .Setup(x => x.Decrypt(It.IsAny<string>()))
            .Throws(new Exception("Database error"));

        var credential = TestDataBuilder.CreateServerCredential(id: 1, username: "encrypted_user");
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().BeNull();

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_EncryptsAndSaves_WhenValidCredential()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential(
            username: "plainuser",
            password: "plainpass"
        );

        // Act
        var id = await _repository.AddAsync(credential);

        // Assert
        id.Should().BeGreaterThan(0);

        // Verify data was encrypted before saving
        var savedCredential = await _context.ServerCredentials.FindAsync(id);
        savedCredential.Should().NotBeNull();
        savedCredential!.Username.Should().Be("encrypted_plainuser");
        savedCredential.Password.Should().Be("encrypted_plainpass");

        _mockCredentialService.Verify(x => x.Encrypt("plainuser"), Times.Once);
        _mockCredentialService.Verify(x => x.Encrypt("plainpass"), Times.Once);
    }

    [Fact]
    public async Task AddAsync_SetsCreatedAt_ToUtcNow()
    {
        // Arrange
        var beforeAdd = DateTime.UtcNow.AddSeconds(-1);
        var credential = TestDataBuilder.CreateServerCredential();

        // Act
        var id = await _repository.AddAsync(credential);

        // Assert
        var savedCredential = await _context.ServerCredentials.FindAsync(id);
        savedCredential.Should().NotBeNull();
        savedCredential!.CreatedAt.Should().BeAfter(beforeAdd);
        savedCredential.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task AddAsync_SetsUpdatedAt_ToUtcNow()
    {
        // Arrange
        var beforeAdd = DateTime.UtcNow.AddSeconds(-1);
        var credential = TestDataBuilder.CreateServerCredential();

        // Act
        var id = await _repository.AddAsync(credential);

        // Assert
        var savedCredential = await _context.ServerCredentials.FindAsync(id);
        savedCredential.Should().NotBeNull();
        savedCredential!.UpdatedAt.Should().BeAfter(beforeAdd);
        savedCredential.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task AddAsync_SetsEncryptionKeyId_FromService()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential();

        // Act
        var id = await _repository.AddAsync(credential);

        // Assert
        var savedCredential = await _context.ServerCredentials.FindAsync(id);
        savedCredential.Should().NotBeNull();
        savedCredential!.EncryptionKeyId.Should().Be("TEST1234");

        _mockCredentialService.Verify(x => x.GetEncryptionKeyId(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ReturnsCredentialId_AfterSave()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential();

        // Act
        var id = await _repository.AddAsync(credential);

        // Assert
        id.Should().BeGreaterThan(0);
        credential.Id.Should().Be(id);
    }

    [Fact]
    public async Task AddAsync_EncryptsNullFieldsAsNull()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential(
            username: null,
            password: "password",
            additionalConfig: null
        );

        // Act
        var id = await _repository.AddAsync(credential);

        // Assert
        var saved = await _context.ServerCredentials.FindAsync(id);
        saved.Should().NotBeNull();
        saved!.Username.Should().BeNull();
        saved.Password.Should().Be("encrypted_password");
        saved.AdditionalConfig.Should().BeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_EncryptsAndUpdates_WhenExists()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential(
            username: "encrypted_olduser",
            password: "encrypted_oldpass"
        );
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        var updatedCredential = new ServerCredential
        {
            Id = credential.Id,
            Username = "newuser",
            Password = "newpass",
            AdditionalConfig = null
        };

        // Act
        var result = await _repository.UpdateAsync(updatedCredential);

        // Assert
        result.Should().BeTrue();

        var saved = await _context.ServerCredentials.FindAsync(credential.Id);
        saved.Should().NotBeNull();
        saved!.Username.Should().Be("encrypted_newuser");
        saved.Password.Should().Be("encrypted_newpass");

        _mockCredentialService.Verify(x => x.Encrypt("newuser"), Times.Once);
        _mockCredentialService.Verify(x => x.Encrypt("newpass"), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        var credential = new ServerCredential { Id = 999 };

        // Act
        var result = await _repository.UpdateAsync(credential);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_LogsWarning_WhenNotFound()
    {
        // Arrange
        var credential = new ServerCredential { Id = 999 };

        // Act
        await _repository.UpdateAsync(credential);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found for update")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesTimestamp_ToUtcNow()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential();
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        await Task.Delay(100); // Small delay to ensure time difference
        var beforeUpdate = DateTime.UtcNow;

        var updatedCredential = new ServerCredential
        {
            Id = credential.Id,
            Username = "newuser"
        };

        // Act
        await _repository.UpdateAsync(updatedCredential);

        // Assert
        var saved = await _context.ServerCredentials.FindAsync(credential.Id);
        saved.Should().NotBeNull();
        saved!.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
        saved.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task UpdateAsync_EncryptsAllFields_WhenNotNull()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential();
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        var updatedCredential = new ServerCredential
        {
            Id = credential.Id,
            Username = "user",
            Password = "pass",
            AdditionalConfig = "config"
        };

        // Act
        await _repository.UpdateAsync(updatedCredential);

        // Assert
        _mockCredentialService.Verify(x => x.Encrypt("user"), Times.Once);
        _mockCredentialService.Verify(x => x.Encrypt("pass"), Times.Once);
        _mockCredentialService.Verify(x => x.Encrypt("config"), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_HandlesNullFields_Correctly()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential(
            username: "encrypted_olduser",
            password: "encrypted_oldpass"
        );
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        var updatedCredential = new ServerCredential
        {
            Id = credential.Id,
            Username = null,
            Password = "newpass",
            AdditionalConfig = null
        };

        // Act
        await _repository.UpdateAsync(updatedCredential);

        // Assert
        var saved = await _context.ServerCredentials.FindAsync(credential.Id);
        saved.Should().NotBeNull();
        saved!.Username.Should().BeNull();
        saved.Password.Should().Be("encrypted_newpass");
        saved.AdditionalConfig.Should().BeNull();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_RemovesCredential_WhenExists()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential();
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();
        var id = credential.Id;

        // Act
        var result = await _repository.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();

        var deleted = await _context.ServerCredentials.FindAsync(id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenSuccessful()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential();
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(credential.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_LogsWarning_WhenNotFound()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found for deletion")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region UpdateLastUsedAsync Tests

    [Fact]
    public async Task UpdateLastUsedAsync_UpdatesTimestamp_WhenExists()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential();
        credential.LastUsedAt = null;
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _repository.UpdateLastUsedAsync(credential.Id);

        // Assert
        result.Should().BeTrue();

        var updated = await _context.ServerCredentials.FindAsync(credential.Id);
        updated.Should().NotBeNull();
        updated!.LastUsedAt.Should().NotBeNull();
        updated.LastUsedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public async Task UpdateLastUsedAsync_ReturnsFalse_WhenNotFound()
    {
        // Act
        var result = await _repository.UpdateLastUsedAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateLastUsedAsync_SetsUtcTime_WhenSuccessful()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential();
        _context.ServerCredentials.Add(credential);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateLastUsedAsync(credential.Id);

        // Assert
        var updated = await _context.ServerCredentials.FindAsync(credential.Id);
        updated!.LastUsedAt.Should().NotBeNull();
        updated.LastUsedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task UpdateLastUsedAsync_LogsWarning_WhenNotFound()
    {
        // Act
        var result = await _repository.UpdateLastUsedAsync(999);

        // Assert
        result.Should().BeFalse();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found for updating LastUsedAt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
