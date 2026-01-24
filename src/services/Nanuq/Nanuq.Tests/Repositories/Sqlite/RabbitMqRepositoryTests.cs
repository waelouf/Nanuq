using FluentAssertions;
using Moq;
using Nanuq.Common.Enums;
using Nanuq.Sqlite.Repositories;
using Nanuq.Tests.Helpers;
using Xunit;

namespace Nanuq.Tests.Repositories.Sqlite;

public class RabbitMqRepositoryTests : SqliteRepositoryTestsBase<RabbitMqRepository>
{
    private readonly RabbitMqRepository _repository;

    public RabbitMqRepositoryTests()
    {
        _repository = new RabbitMqRepository(MockLogger.Object, Context, MockAuditLog.Object);
    }

    [Fact]
    public async Task Add_InsertsRecord_WhenValid()
    {
        // Arrange
        var record = TestDataBuilder.CreateRabbitMQRecord();

        // Act
        var id = await _repository.Add(record);

        // Assert
        id.Should().BeGreaterThan(0);

        var saved = await Context.RabbitMQ.FindAsync(id);
        saved.Should().NotBeNull();
        saved!.ServerUrl.Should().Be("localhost:5672");
        saved.Alias.Should().Be("test-rabbitmq-server");
        saved.Environment.Should().Be("Development");
    }

    [Fact]
    public async Task Add_CallsAuditLog_WhenSuccessful()
    {
        // Arrange
        var record = TestDataBuilder.CreateRabbitMQRecord(
            serverUrl: "prod-rabbitmq:5672",
            alias: "production-rabbitmq",
            environment: "Production"
        );

        // Act
        await _repository.Add(record);

        // Assert
        MockAuditLog.Verify(
            x => x.Audit(
                ActivityTypeEnum.AddRabbitMQServer,
                It.Is<string>(s => s.Contains("production-rabbitmq") && s.Contains("prod-rabbitmq:5672") && s.Contains("Production")),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Add_SetsGeneratedId_AfterSave()
    {
        // Arrange
        var record = TestDataBuilder.CreateRabbitMQRecord();

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
        var record1 = TestDataBuilder.CreateRabbitMQRecord(alias: "rabbitmq-1");
        var record2 = TestDataBuilder.CreateRabbitMQRecord(alias: "rabbitmq-2");
        var record3 = TestDataBuilder.CreateRabbitMQRecord(alias: "rabbitmq-3");

        await _repository.Add(record1);
        await _repository.Add(record2);
        await _repository.Add(record3);

        // Act
        var results = await _repository.GetAll();

        // Assert
        results.Should().HaveCount(3);
        results.Should().Contain(r => r.Alias == "rabbitmq-1");
        results.Should().Contain(r => r.Alias == "rabbitmq-2");
        results.Should().Contain(r => r.Alias == "rabbitmq-3");
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
        var record = TestDataBuilder.CreateRabbitMQRecord();
        var id = await _repository.Add(record);

        // Act
        var result = await _repository.Get(id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.ServerUrl.Should().Be("localhost:5672");
        result.Alias.Should().Be("test-rabbitmq-server");
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
        var record = TestDataBuilder.CreateRabbitMQRecord();
        var id = await _repository.Add(record);

        // Act
        var result = await _repository.Delete(id);

        // Assert
        result.Should().BeTrue();

        var deleted = await Context.RabbitMQ.FindAsync(id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Delete_CallsAuditLog_BeforeDeletion()
    {
        // Arrange
        var record = TestDataBuilder.CreateRabbitMQRecord(
            alias: "to-delete-rabbitmq"
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
                ActivityTypeEnum.RemoveRabbitMQServer,
                It.Is<string>(s => s.Contains("to-delete-rabbitmq")),
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
