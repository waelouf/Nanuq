using FluentAssertions;
using Moq;
using Nanuq.Common.Enums;
using Nanuq.Sqlite.Repositories;
using Nanuq.Tests.Helpers;
using Xunit;

namespace Nanuq.Tests.Repositories.Sqlite;

public class AwsRepositoryTests : SqliteRepositoryTestsBase<AwsRepository>
{
    private readonly AwsRepository _repository;

    public AwsRepositoryTests()
    {
        _repository = new AwsRepository(MockLogger.Object, Context, MockAuditLog.Object);
    }

    [Fact]
    public async Task Add_InsertsRecord_WhenValid()
    {
        // Arrange
        var record = TestDataBuilder.CreateAwsRecord();

        // Act
        var id = await _repository.Add(record);

        // Assert
        id.Should().BeGreaterThan(0);

        var saved = await _repository.Get(id);
        saved.Should().NotBeNull();
        saved!.Region.Should().Be("us-east-1");
        saved.Alias.Should().Be("test-aws-server");
        saved.Environment.Should().Be("Development");
        saved.ServiceType.Should().Be("SQS");
    }

    [Fact]
    public async Task Add_CallsAuditLog_WhenSuccessful()
    {
        // Arrange
        var record = TestDataBuilder.CreateAwsRecord(
            region: "eu-west-1",
            alias: "production-aws",
            environment: "Production",
            serviceType: "SNS"
        );

        // Act
        await _repository.Add(record);

        // Assert
        MockAuditLog.Verify(
            x => x.Audit(
                ActivityTypeEnum.AddAWSServer,
                It.Is<string>(s => s.Contains("production-aws") && s.Contains("eu-west-1") && s.Contains("Production") && s.Contains("SNS")),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Add_SetsGeneratedId_AfterSave()
    {
        // Arrange
        var record = TestDataBuilder.CreateAwsRecord();

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
        var record1 = TestDataBuilder.CreateAwsRecord(alias: "aws-1");
        var record2 = TestDataBuilder.CreateAwsRecord(alias: "aws-2");
        var record3 = TestDataBuilder.CreateAwsRecord(alias: "aws-3");

        await _repository.Add(record1);
        await _repository.Add(record2);
        await _repository.Add(record3);

        // Act
        var results = await _repository.GetAll();

        // Assert
        results.Should().HaveCount(3);
        results.Should().Contain(r => r.Alias == "aws-1");
        results.Should().Contain(r => r.Alias == "aws-2");
        results.Should().Contain(r => r.Alias == "aws-3");
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
        var record = TestDataBuilder.CreateAwsRecord();
        var id = await _repository.Add(record);

        // Act
        var result = await _repository.Get(id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Region.Should().Be("us-east-1");
        result.Alias.Should().Be("test-aws-server");
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
        var record = TestDataBuilder.CreateAwsRecord();
        var id = await _repository.Add(record);

        var updatedRecord = TestDataBuilder.CreateAwsRecord(
            region: "ap-southeast-1",
            alias: "updated-aws",
            environment: "Staging",
            serviceType: "SNS"
        );
        updatedRecord.Id = id;

        // Act
        var result = await _repository.Update(updatedRecord);

        // Assert
        result.Should().BeTrue();

        var saved = await _repository.Get(id);
        saved.Should().NotBeNull();
        saved!.Region.Should().Be("ap-southeast-1");
        saved.Alias.Should().Be("updated-aws");
        saved.Environment.Should().Be("Staging");
        saved.ServiceType.Should().Be("SNS");
    }

    [Fact]
    public async Task Update_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        var record = TestDataBuilder.CreateAwsRecord();
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
        var record = TestDataBuilder.CreateAwsRecord();
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
        var record = TestDataBuilder.CreateAwsRecord(
            alias: "to-delete-aws"
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
                ActivityTypeEnum.RemoveAWSServer,
                It.Is<string>(s => s.Contains("to-delete-aws")),
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
