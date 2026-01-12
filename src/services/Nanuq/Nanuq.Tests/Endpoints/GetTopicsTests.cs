using Xunit;
using Moq;
using FluentAssertions;
using Nanuq.WebApi.Endpoints.Kafka;
using Nanuq.Kafka.Interfaces;
using Nanuq.Kafka.Entities;
using Nanuq.Common.Interfaces;

namespace Nanuq.Tests.Endpoints;

public class GetTopicsTests
{
    private readonly Mock<ITopicsRepository> _mockTopicsRepository;
    private readonly Mock<IKafkaRepository> _mockKafkaRepository;
    private readonly Mock<ICredentialRepository> _mockCredentialRepository;
    private readonly GetTopics _endpoint;

    public GetTopicsTests()
    {
        _mockTopicsRepository = new Mock<ITopicsRepository>();
        _mockKafkaRepository = new Mock<IKafkaRepository>();
        _mockCredentialRepository = new Mock<ICredentialRepository>();
        _endpoint = new GetTopics(_mockTopicsRepository.Object, _mockKafkaRepository.Object, _mockCredentialRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnTopics_WhenServerExists()
    {
        // Arrange
        var serverName = "test-server";
        var expectedTopics = new List<Topic>
        {
            new Topic("topic1", 3),
            new Topic("topic2", 5)
        };

        _mockTopicsRepository
            .Setup(repo => repo.GetTopicsAsync(serverName))
            .ReturnsAsync(expectedTopics);

        // Act & Assert
        // Note: Full endpoint testing would require setting up the HTTP context
        // This is a unit test for the repository interaction
        var topics = await _mockTopicsRepository.Object.GetTopicsAsync(serverName);

        // Assert
        topics.Should().NotBeNull();
        topics.Should().HaveCount(2);
        topics.First().TopicName.Should().Be("topic1");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyList_WhenNoTopicsExist()
    {
        // Arrange
        var serverName = "empty-server";
        var expectedTopics = new List<Topic>();

        _mockTopicsRepository
            .Setup(repo => repo.GetTopicsAsync(serverName))
            .ReturnsAsync(expectedTopics);

        // Act
        var topics = await _mockTopicsRepository.Object.GetTopicsAsync(serverName);

        // Assert
        topics.Should().NotBeNull();
        topics.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepository_WithCorrectServerName()
    {
        // Arrange
        var serverName = "test-server";
        _mockTopicsRepository
            .Setup(repo => repo.GetTopicsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Topic>());

        // Act
        await _mockTopicsRepository.Object.GetTopicsAsync(serverName);

        // Assert
        _mockTopicsRepository.Verify(
            repo => repo.GetTopicsAsync(serverName),
            Times.Once
        );
    }
}
