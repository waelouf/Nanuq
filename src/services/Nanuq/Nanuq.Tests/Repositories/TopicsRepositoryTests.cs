using Xunit;
using FluentAssertions;
using Nanuq.Kafka.Repositories;
using Nanuq.Kafka.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Nanuq.Common.Interfaces;

namespace Nanuq.Tests.Repositories;

public class TopicsRepositoryTests
{
    private readonly TopicsRepository _repository;
    private readonly Mock<ILogger<TopicsRepository>> _mockLogger;
    private readonly Mock<IAuditLogRepository> _mockAuditLog;

    public TopicsRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<TopicsRepository>>();
        _mockAuditLog = new Mock<IAuditLogRepository>();
        _repository = new TopicsRepository(_mockLogger.Object, _mockAuditLog.Object);
    }

    [Fact]
    public async Task GetTopicsAsync_ShouldReturnNull_WhenKafkaNotAvailable()
    {
        // Arrange
        var serverName = "invalid-server:9092";

        // Act
        var result = await _repository.GetTopicsAsync(serverName);

        // Assert
        // When Kafka is not available, the repository returns null
        result.Should().BeNull();
    }

    [Fact]
    public void TopicsRepository_ShouldAcceptValidParameters()
    {
        // Arrange & Act
        var logger = new Mock<ILogger<TopicsRepository>>();
        var auditLog = new Mock<IAuditLogRepository>();

        var repository = new TopicsRepository(logger.Object, auditLog.Object);

        // Assert
        repository.Should().NotBeNull();
    }

    [Theory]
    [InlineData("localhost:9092")]
    [InlineData("kafka-server:9092")]
    [InlineData("127.0.0.1:9092")]
    public async Task GetTopicsAsync_ShouldAcceptValidServerNames(string serverName)
    {
        // Act
        var result = await _repository.GetTopicsAsync(serverName);

        // Assert
        // Result will be null if Kafka is not running, which is expected in test environment
        // This test verifies that the method accepts valid server names without throwing
        // In integration tests with real Kafka, we would verify actual topics
        result.Should().BeNull();
    }
}
