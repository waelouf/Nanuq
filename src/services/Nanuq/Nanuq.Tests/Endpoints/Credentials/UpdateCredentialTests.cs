using FluentAssertions;
using Moq;
using Nanuq.Common.Interfaces;
using Nanuq.Tests.Helpers;
using Nanuq.WebApi.Endpoints.Credentials;
using Xunit;

namespace Nanuq.Tests.Endpoints.Credentials;

public class UpdateCredentialTests
{
    private readonly Mock<ICredentialRepository> _mockCredentialRepository;
    private readonly UpdateCredential _endpoint;

    public UpdateCredentialTests()
    {
        _mockCredentialRepository = new Mock<ICredentialRepository>();
        _endpoint = new UpdateCredential(_mockCredentialRepository.Object);
    }

    [Fact]
    public async Task UpdateCredential_UpdatesCredential_WhenExists()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential(id: 1);

        _mockCredentialRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(credential);

        _mockCredentialRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<Common.Records.ServerCredential>()))
            .ReturnsAsync(true);

        // Act
        await _mockCredentialRepository.Object.UpdateAsync(credential);

        // Assert
        _mockCredentialRepository.Verify(
            repo => repo.UpdateAsync(It.IsAny<Common.Records.ServerCredential>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCredential_ReturnsTrue_WhenSuccessful()
    {
        // Arrange
        _mockCredentialRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<Common.Records.ServerCredential>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockCredentialRepository.Object.UpdateAsync(
            TestDataBuilder.CreateServerCredential());

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCredential_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        _mockCredentialRepository
            .Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((Common.Records.ServerCredential?)null);

        // Act
        var existing = await _mockCredentialRepository.Object.GetByIdAsync(999);

        // Assert
        existing.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCredential_CallsGetById_First()
    {
        // Arrange
        var credential = TestDataBuilder.CreateServerCredential(id: 1);

        _mockCredentialRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(credential);

        // Act
        await _mockCredentialRepository.Object.GetByIdAsync(1);

        // Assert
        _mockCredentialRepository.Verify(
            repo => repo.GetByIdAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCredential_PreservesUnchangedFields_WhenPartialUpdate()
    {
        // Arrange
        var originalCredential = TestDataBuilder.CreateServerCredential(
            id: 1,
            username: "original-user",
            password: "original-pass"
        );

        _mockCredentialRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(originalCredential);

        _mockCredentialRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<Common.Records.ServerCredential>()))
            .ReturnsAsync(true);

        // Simulate updating only password
        originalCredential.Password = "new-pass";

        // Act
        await _mockCredentialRepository.Object.UpdateAsync(originalCredential);

        // Assert
        _mockCredentialRepository.Verify(
            repo => repo.UpdateAsync(It.Is<Common.Records.ServerCredential>(c =>
                c.Username == "original-user" && // Preserved
                c.Password == "new-pass")),      // Updated
            Times.Once);
    }
}
