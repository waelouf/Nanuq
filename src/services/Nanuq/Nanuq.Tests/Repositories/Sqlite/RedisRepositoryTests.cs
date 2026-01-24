using FluentAssertions;
using Moq;
using Nanuq.Common.Enums;
using Nanuq.Sqlite.Repositories;
using Nanuq.Tests.Helpers;
using Xunit;

namespace Nanuq.Tests.Repositories.Sqlite;

public class RedisRepositoryTests : SqliteRepositoryTestsBase<RedisRepository>
{
    private readonly RedisRepository _repository;

    public RedisRepositoryTests()
    {
        _repository = new RedisRepository(Context, MockLogger.Object, MockAuditLog.Object);
    }

    [Fact]
    public async Task Add_InsertsRecord_WhenValid()
    {
        // Arrange
        var record = TestDataBuilder.CreateRedisRecord();

        // Act
        var id = await _repository.Add(record);

        // Assert
        id.Should().BeGreaterThan(0);

        var saved = await Context.Redis.FindAsync(id);
        saved.Should().NotBeNull();
        saved!.ServerUrl.Should().Be("localhost:6379");
        saved.Alias.Should().Be("test-redis-server");
        saved.Environment.Should().Be("Development");
    }

    [Fact]
    public async Task Add_CallsAuditLog_WhenSuccessful()
    {
        // Arrange
        var record = TestDataBuilder.CreateRedisRecord(
            serverUrl: "prod-redis:6379",
            alias: "production-redis",
            environment: "Production"
        );

        // Act
        await _repository.Add(record);

        // Assert
        MockAuditLog.Verify(
            x => x.Audit(
                ActivityTypeEnum.AddRedisServer,
                It.Is<string>(s => s.Contains("production-redis") && s.Contains("prod-redis:6379") && s.Contains("Production")),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Add_SetsGeneratedId_AfterSave()
    {
        // Arrange
        var record = TestDataBuilder.CreateRedisRecord();

        // Act
        var id = await _repository.Add(record);

        // Assert
        record.Id.Should().Be(id);
        record.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAll_ReturnsAllRecords_WhenMultipleExist()
    {
        // Arrange
        var record1 = TestDataBuilder.CreateRedisRecord(alias: "redis-1");
        var record2 = TestDataBuilder.CreateRedisRecord(alias: "redis-2");
        var record3 = TestDataBuilder.CreateRedisRecord(alias: "redis-3");

        await _repository.Add(record1);
        await _repository.Add(record2);
        await _repository.Add(record3);

        // Act
        var results = await _repository.GetAll();

        // Assert
        results.Should().HaveCount(3);
        results.Should().Contain(r => r.Alias == "redis-1");
        results.Should().Contain(r => r.Alias == "redis-2");
        results.Should().Contain(r => r.Alias == "redis-3");
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoRecordsExist()
    {
        // Act
        var results = await _repository.GetAll();

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_ReturnsRecord_WhenExists()
    {
        // Arrange
        var record = TestDataBuilder.CreateRedisRecord();
        var id = await _repository.Add(record);

        // Act
        var result = await _repository.Get(id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.ServerUrl.Should().Be("localhost:6379");
        result.Alias.Should().Be("test-redis-server");
    }

    [Fact]
    public async Task Get_ReturnsNull_WhenNotFound()
    {
        // Act
        var result = await _repository.Get(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Delete_RemovesRecord_WhenExists()
    {
        // Arrange
        var record = TestDataBuilder.CreateRedisRecord();
        var id = await _repository.Add(record);

        // Act
        var result = await _repository.Delete(id);

        // Assert
        result.Should().BeTrue();

        var deleted = await Context.Redis.FindAsync(id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Delete_CallsAuditLog_BeforeDeletion()
    {
        // Arrange
        var record = TestDataBuilder.CreateRedisRecord(
            alias: "to-delete-redis"
        );
        var id = await _repository.Add(record);

        // Reset mock to clear the Add audit call
        MockAuditLog.Reset();
        MockAuditLog
            .Setup(x => x.Audit(It.IsAny<ActivityTypeEnum>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(1);

        // Act
        await _repository.Delete(id);

        // Assert
        MockAuditLog.Verify(
            x => x.Audit(
                ActivityTypeEnum.RemoveRedisServer,
                It.Is<string>(s => s.Contains("to-delete-redis")),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsFalse_WhenNotFound()
    {
        // Act
        var result = await _repository.Delete(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_DoesNotCallAuditLog_WhenNotFound()
    {
        // Act
        await _repository.Delete(999);

        // Assert
        MockAuditLog.Verify(
            x => x.Audit(It.IsAny<ActivityTypeEnum>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }
}
