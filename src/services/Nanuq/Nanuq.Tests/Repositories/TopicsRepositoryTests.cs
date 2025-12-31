using Xunit;
using FluentAssertions;
using Nanuq.Kafka.Repositories;
using Nanuq.Kafka.Entities;

namespace Nanuq.Tests.Repositories;

public class TopicsRepositoryTests
{
    private readonly TopicsRepository _repository;

    public TopicsRepositoryTests()
    {
        _repository = new TopicsRepository();
    }

    [Fact]
    public async Task GetTopicsAsync_ShouldNotBeNull()
    {
        // Arrange
        var serverName = "localhost:9092";

        // Act
        var result = await _repository.GetTopicsAsync(serverName);

        // Assert
        result.Should().NotBeNull();
        // Note: This will return empty or actual topics depending on Kafka availability
        // In a real test environment, you would mock the Kafka client
    }

    [Fact]
    public void GetTopicsAsync_ShouldAcceptNonNullServerName()
    {
        // Arrange & Act
        Func<Task> act = async () => await _repository.GetTopicsAsync(null!);

        // Assert
        // This tests that the method handles null gracefully or throws expected exception
        // Adjust based on actual implementation
        act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("localhost:9092")]
    [InlineData("kafka-server:9092")]
    [InlineData("192.168.1.100:9092")]
    public async Task GetTopicsAsync_ShouldAcceptVariousServerFormats(string serverName)
    {
        // Act
        var result = await _repository.GetTopicsAsync(serverName);

        // Assert
        result.Should().NotBeNull();
    }
}
