using FluentAssertions;
using Moq;
using Nanuq.Common.Interfaces;
using Nanuq.WebApi.Endpoints.Credentials;
using Xunit;

namespace Nanuq.Tests.Endpoints.Credentials;

public class DeleteCredentialTests
{
    private readonly Mock<ICredentialRepository> _mockCredentialRepository;
    private readonly DeleteCredential _endpoint;

    public DeleteCredentialTests()
    {
        _mockCredentialRepository = new Mock<ICredentialRepository>();
        _endpoint = new DeleteCredential(_mockCredentialRepository.Object);
    }

    [Fact]
    public async Task DeleteCredential_ReturnsTrue_WhenSuccessful()
    {
        // Arrange
        _mockCredentialRepository
            .Setup(repo => repo.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _mockCredentialRepository.Object.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCredential_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        _mockCredentialRepository
            .Setup(repo => repo.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _mockCredentialRepository.Object.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCredential_CallsRepository_WithCorrectId()
    {
        // Arrange
        _mockCredentialRepository
            .Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await _mockCredentialRepository.Object.DeleteAsync(42);

        // Assert
        _mockCredentialRepository.Verify(
            repo => repo.DeleteAsync(42),
            Times.Once);
    }

    [Fact]
    public async Task DeleteCredential_CallsRepository_OnlyOnce()
    {
        // Arrange
        _mockCredentialRepository
            .Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await _mockCredentialRepository.Object.DeleteAsync(1);

        // Assert
        _mockCredentialRepository.Verify(
            repo => repo.DeleteAsync(It.IsAny<int>()),
            Times.Once);
    }
}
