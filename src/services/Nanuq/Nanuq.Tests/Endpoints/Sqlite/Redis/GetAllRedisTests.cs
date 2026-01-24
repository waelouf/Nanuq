using FluentAssertions;
using Moq;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Tests.Helpers;
using Xunit;

namespace Nanuq.Tests.Endpoints.Sqlite.Redis;

public class GetAllRedisTests
{
    private readonly Mock<IRedisRepository> _mockRedisRepository;

    public GetAllRedisTests()
    {
        _mockRedisRepository = new Mock<IRedisRepository>();
    }

    [Fact]
    public async Task GetAll_ReturnsAllRecords_WhenRecordsExist()
    {
        // Arrange
        var records = new List<RedisRecord>
        {
            TestDataBuilder.CreateRedisRecord(alias: "redis-1"),
            TestDataBuilder.CreateRedisRecord(alias: "redis-2")
        };

        _mockRedisRepository
            .Setup(repo => repo.GetAll())
            .ReturnsAsync(records);

        // Act
        var result = await _mockRedisRepository.Object.GetAll();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Alias == "redis-1");
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoRecordsExist()
    {
        // Arrange
        _mockRedisRepository
            .Setup(repo => repo.GetAll())
            .ReturnsAsync(new List<RedisRecord>());

        // Act
        var result = await _mockRedisRepository.Object.GetAll();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_CallsRepository_Once()
    {
        // Arrange
        _mockRedisRepository
            .Setup(repo => repo.GetAll())
            .ReturnsAsync(new List<RedisRecord>());

        // Act
        await _mockRedisRepository.Object.GetAll();

        // Assert
        _mockRedisRepository.Verify(repo => repo.GetAll(), Times.Once);
    }
}
