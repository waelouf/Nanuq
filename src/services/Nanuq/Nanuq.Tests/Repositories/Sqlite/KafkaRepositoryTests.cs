using FluentAssertions;
using Moq;
using Nanuq.Common.Enums;
using Nanuq.Common.Repositories;
using Nanuq.Tests.Helpers;
using Xunit;

namespace Nanuq.Tests.Repositories.Sqlite;

public class KafkaRepositoryTests : SqliteRepositoryTestsBase<KafkaRepository>
{
    private readonly KafkaRepository _repository;

    public KafkaRepositoryTests()
    {
        _repository = new KafkaRepository(MockLogger.Object, Context, MockAuditLog.Object);
    }

    [Fact]
    public async Task Add_InsertsRecord_WhenValid()
    {
        // Arrange
        var record = TestDataBuilder.CreateKafkaRecord();

        // Act
        var id = await _repository.Add(record);

        // Assert
        id.Should().BeGreaterThan(0);

        var saved = await Context.Kafka.FindAsync(id);
        saved.Should().NotBeNull();
        saved!.BootstrapServer.Should().Be("localhost:9092");
        saved.Alias.Should().Be("test-kafka-server");
        saved.Environment.Should().Be("Development");
    }

    [Fact]
    public async Task Add_CallsAuditLog_WhenSuccessful()
    {
        // Arrange
        var record = TestDataBuilder.CreateKafkaRecord(
            bootstrapServer: "prod-kafka:9092",
            alias: "production-kafka",
            environment: "Production"
        );

        // Act
        await _repository.Add(record);

        // Assert
        MockAuditLog.Verify(
            x => x.Audit(
                ActivityTypeEnum.AddKafkaServer,
                It.Is<string>(s => s.Contains("production-kafka") && s.Contains("prod-kafka:9092") && s.Contains("Production")),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Add_SetsGeneratedId_AfterSave()
    {
        // Arrange
        var record = TestDataBuilder.CreateKafkaRecord();

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
        var record1 = TestDataBuilder.CreateKafkaRecord(alias: "kafka-1");
        var record2 = TestDataBuilder.CreateKafkaRecord(alias: "kafka-2");
        var record3 = TestDataBuilder.CreateKafkaRecord(alias: "kafka-3");

        await _repository.Add(record1);
        await _repository.Add(record2);
        await _repository.Add(record3);

        // Act
        var results = await _repository.GetAll();

        // Assert
        results.Should().HaveCount(3);
        results.Should().Contain(r => r.Alias == "kafka-1");
        results.Should().Contain(r => r.Alias == "kafka-2");
        results.Should().Contain(r => r.Alias == "kafka-3");
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
        var record = TestDataBuilder.CreateKafkaRecord();
        var id = await _repository.Add(record);

        // Act
        var result = await _repository.Get(id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.BootstrapServer.Should().Be("localhost:9092");
        result.Alias.Should().Be("test-kafka-server");
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
    public async Task Update_ModifiesRecord_WhenExists()
    {
        // Arrange
        var record = TestDataBuilder.CreateKafkaRecord();
        var id = await _repository.Add(record);

        var updatedRecord = TestDataBuilder.CreateKafkaRecord(
            bootstrapServer: "updated-kafka:9092",
            alias: "updated-alias",
            environment: "Production"
        );
        updatedRecord.Id = id;

        // Act
        var result = await _repository.Update(updatedRecord);

        // Assert
        result.Should().BeTrue();

        var saved = await Context.Kafka.FindAsync(id);
        saved.Should().NotBeNull();
        saved!.BootstrapServer.Should().Be("updated-kafka:9092");
        saved.Alias.Should().Be("updated-alias");
        saved.Environment.Should().Be("Production");
    }

    [Fact]
    public async Task Update_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        var record = TestDataBuilder.CreateKafkaRecord();
        record.Id = 999;

        // Act
        var result = await _repository.Update(record);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_RemovesRecord_WhenExists()
    {
        // Arrange
        var record = TestDataBuilder.CreateKafkaRecord();
        var id = await _repository.Add(record);

        // Act
        var result = await _repository.Delete(id);

        // Assert
        result.Should().BeTrue();

        var deleted = await Context.Kafka.FindAsync(id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Delete_CallsAuditLog_BeforeDeletion()
    {
        // Arrange
        var record = TestDataBuilder.CreateKafkaRecord(
            alias: "to-delete-kafka"
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
                ActivityTypeEnum.RemoveKafkaServer,
                It.Is<string>(s => s.Contains("to-delete-kafka")),
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
