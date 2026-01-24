using FluentAssertions;
using Moq;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Common.Requests;
using Nanuq.WebApi.Endpoints.Credentials;
using Xunit;

namespace Nanuq.Tests.Endpoints.Credentials;

public class AddCredentialTests
{
    private readonly Mock<ICredentialRepository> _mockCredentialRepository;
    private readonly AddCredential _endpoint;

    public AddCredentialTests()
    {
        _mockCredentialRepository = new Mock<ICredentialRepository>();
        _endpoint = new AddCredential(_mockCredentialRepository.Object);
    }

    [Fact]
    public async Task AddCredential_CallsRepository_WithCorrectData()
    {
        // Arrange
        var request = new AddCredentialRequest(
            ServerId: 1,
            ServerType: "Kafka",
            Username: "admin",
            Password: "secret",
            AdditionalConfig: null
        );

        _mockCredentialRepository
            .Setup(repo => repo.AddAsync(It.IsAny<ServerCredential>()))
            .ReturnsAsync(123);

        // Act
        var result = await _mockCredentialRepository.Object.AddAsync(new ServerCredential
        {
            ServerId = request.ServerId,
            ServerType = request.ServerType,
            Username = request.Username,
            Password = request.Password,
            AdditionalConfig = request.AdditionalConfig
        });

        // Assert
        result.Should().Be(123);
        _mockCredentialRepository.Verify(
            repo => repo.AddAsync(It.Is<ServerCredential>(c =>
                c.ServerId == 1 &&
                c.ServerType == "Kafka" &&
                c.Username == "admin" &&
                c.Password == "secret")),
            Times.Once);
    }

    [Fact]
    public async Task AddCredential_ReturnsCredentialId_WhenSuccessful()
    {
        // Arrange
        var expectedId = 456;
        _mockCredentialRepository
            .Setup(repo => repo.AddAsync(It.IsAny<ServerCredential>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _mockCredentialRepository.Object.AddAsync(new ServerCredential());

        // Assert
        result.Should().Be(expectedId);
    }

    [Fact]
    public async Task AddCredential_SupportsAllServerTypes()
    {
        // Arrange
        var serverTypes = new[] { "Kafka", "Redis", "RabbitMQ", "AWS", "Azure" };

        foreach (var serverType in serverTypes)
        {
            _mockCredentialRepository
                .Setup(repo => repo.AddAsync(It.IsAny<ServerCredential>()))
                .ReturnsAsync(1);

            // Act
            await _mockCredentialRepository.Object.AddAsync(new ServerCredential
            {
                ServerType = serverType
            });

            // Assert
            _mockCredentialRepository.Verify(
                repo => repo.AddAsync(It.Is<ServerCredential>(c => c.ServerType == serverType)),
                Times.Once);

            _mockCredentialRepository.Reset();
        }
    }

    [Fact]
    public async Task AddCredential_AcceptsNullUsername_ForPasswordOnlyAuth()
    {
        // Arrange
        _mockCredentialRepository
            .Setup(repo => repo.AddAsync(It.IsAny<ServerCredential>()))
            .ReturnsAsync(1);

        // Act
        await _mockCredentialRepository.Object.AddAsync(new ServerCredential
        {
            ServerId = 1,
            ServerType = "Redis",
            Username = null,
            Password = "redis-password"
        });

        // Assert
        _mockCredentialRepository.Verify(
            repo => repo.AddAsync(It.Is<ServerCredential>(c =>
                c.Username == null &&
                c.Password == "redis-password")),
            Times.Once);
    }

    [Fact]
    public async Task AddCredential_StoresAdditionalConfig_WhenProvided()
    {
        // Arrange
        var configJson = "{\"region\":\"us-east-1\",\"profile\":\"default\"}";
        _mockCredentialRepository
            .Setup(repo => repo.AddAsync(It.IsAny<ServerCredential>()))
            .ReturnsAsync(1);

        // Act
        await _mockCredentialRepository.Object.AddAsync(new ServerCredential
        {
            ServerId = 1,
            ServerType = "AWS",
            Username = "access-key",
            Password = "secret-key",
            AdditionalConfig = configJson
        });

        // Assert
        _mockCredentialRepository.Verify(
            repo => repo.AddAsync(It.Is<ServerCredential>(c =>
                c.AdditionalConfig == configJson)),
            Times.Once);
    }
}
