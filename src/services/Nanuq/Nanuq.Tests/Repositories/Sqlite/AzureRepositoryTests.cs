using FluentAssertions;
using Moq;
using Nanuq.Common.Enums;
using Nanuq.Sqlite.Repositories;
using Nanuq.Tests.Helpers;
using Xunit;

namespace Nanuq.Tests.Repositories.Sqlite;

public class AzureRepositoryTests : SqliteRepositoryTestsBase<AzureRepository>
{
    private readonly AzureRepository _repository;

    public AzureRepositoryTests()
    {
        _repository = new AzureRepository(MockLogger.Object, Context, MockAuditLog.Object);
    }

    [Fact]
    public async Task Add_InsertsRecord_WhenValid()
    {
        // Arrange
        var record = TestDataBuilder.CreateAzureRecord();

        // Act
        var id = await _repository.Add(record);

        // Assert
        id.Should().BeGreaterThan(0);

        var saved = await _repository.Get(id);
        saved.Should().NotBeNull();
        saved!.Namespace.Should().Be("test-namespace.servicebus.windows.net");
        saved.Alias.Should().Be("test-azure-server");
        saved.Region.Should().Be("East US");
        saved.Environment.Should().Be("Development");
        saved.ServiceType.Should().Be("ServiceBus");
    }

    [Fact]
    public async Task Add_CallsAuditLog_WhenSuccessful()
    {
        // Arrange
        var record = TestDataBuilder.CreateAzureRecord(
            alias: "production-azure",
            azureNamespace: "prod.servicebus.windows.net",
            region: "West Europe",
            environment: "Production"
        );

        // Act
        await _repository.Add(record);

        // Assert
        MockAuditLog.Verify(
            x => x.Audit(
                ActivityTypeEnum.AddAzureServer,
                It.Is<string>(s => s.Contains("production-azure") && s.Contains("prod.servicebus.windows.net") && s.Contains("Production")),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Add_SetsTimestamps_WhenCreated()
    {
        // Arrange
        var beforeAdd = DateTime.UtcNow.AddSeconds(-1);
        var record = TestDataBuilder.CreateAzureRecord();

        // Act
        var id = await _repository.Add(record);

        // Assert
        var saved = await _repository.Get(id);
        saved.Should().NotBeNull();
        saved!.CreatedAt.Should().BeAfter(beforeAdd);
        saved.UpdatedAt.Should().BeAfter(beforeAdd);
        saved.CreatedAt.Should().BeCloseTo(saved.UpdatedAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetAll_ReturnsAllRecords_WhenMultipleExist()
    {
        // Arrange
        var record1 = TestDataBuilder.CreateAzureRecord(alias: "azure-1");
        var record2 = TestDataBuilder.CreateAzureRecord(alias: "azure-2");
        var record3 = TestDataBuilder.CreateAzureRecord(alias: "azure-3");

        await _repository.Add(record1);
        await _repository.Add(record2);
        await _repository.Add(record3);

        // Act
        var results = await _repository.GetAll();

        // Assert
        results.Should().HaveCount(3);
        results.Should().Contain(r => r.Alias == "azure-1");
        results.Should().Contain(r => r.Alias == "azure-2");
        results.Should().Contain(r => r.Alias == "azure-3");
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
        var record = TestDataBuilder.CreateAzureRecord();
        var id = await _repository.Add(record);

        // Act
        var result = await _repository.Get(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Namespace.Should().Be("test-namespace.servicebus.windows.net");
        result.Alias.Should().Be("test-azure-server");
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
        var record = TestDataBuilder.CreateAzureRecord();
        var id = await _repository.Add(record);

        await Task.Delay(100); // Small delay to ensure timestamp difference

        var updatedRecord = TestDataBuilder.CreateAzureRecord(
            alias: "updated-azure",
            azureNamespace: "updated.servicebus.windows.net",
            region: "North Europe",
            environment: "Staging",
            serviceType: "EventHubs"
        );
        updatedRecord.Id = id;

        // Act
        var result = await _repository.Update(updatedRecord);

        // Assert
        result.Should().BeTrue();

        var saved = await _repository.Get(id);
        saved.Should().NotBeNull();
        saved!.Alias.Should().Be("updated-azure");
        saved.Namespace.Should().Be("updated.servicebus.windows.net");
        saved.Region.Should().Be("North Europe");
        saved.Environment.Should().Be("Staging");
        saved.ServiceType.Should().Be("EventHubs");
    }

    [Fact]
    public async Task Update_UpdatesTimestamp_WhenModified()
    {
        // Arrange
        var record = TestDataBuilder.CreateAzureRecord();
        var id = await _repository.Add(record);

        var original = await _repository.Get(id);
        var originalUpdatedAt = original!.UpdatedAt;

        await Task.Delay(100); // Ensure time difference

        var updatedRecord = TestDataBuilder.CreateAzureRecord(alias: "new-alias");
        updatedRecord.Id = id;

        // Act
        await _repository.Update(updatedRecord);

        // Assert
        var saved = await _repository.Get(id);
        saved!.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task Update_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        var record = TestDataBuilder.CreateAzureRecord();
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
        var record = TestDataBuilder.CreateAzureRecord();
        var id = await _repository.Add(record);

        // Act
        var result = await _repository.Delete(id);

        // Assert
        result.Should().BeTrue();

        var deleted = await _repository.Get(id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Delete_CallsAuditLog_BeforeDeletion()
    {
        // Arrange
        var record = TestDataBuilder.CreateAzureRecord(
            alias: "to-delete-azure"
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
                ActivityTypeEnum.RemoveAzureServer,
                It.Is<string>(s => s.Contains("to-delete-azure")),
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
