using FluentAssertions;
using Moq;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Tests.Helpers;
using Nanuq.WebApi.Endpoints.Sqlite.Kafka;
using Xunit;

namespace Nanuq.Tests.Endpoints.Sqlite.Kafka;

public class GetAllKafkaTests
{
    private readonly Mock<IKafkaRepository> _mockKafkaRepository;
    private readonly GetAllKafka _endpoint;

    public GetAllKafkaTests()
    {
        _mockKafkaRepository = new Mock<IKafkaRepository>();
        _endpoint = new GetAllKafka(_mockKafkaRepository.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsAllRecords_WhenRecordsExist()
    {
        // Arrange
        var records = new List<KafkaRecord>
        {
            TestDataBuilder.CreateKafkaRecord(alias: "kafka-1"),
            TestDataBuilder.CreateKafkaRecord(alias: "kafka-2")
        };

        _mockKafkaRepository
            .Setup(repo => repo.GetAll())
            .ReturnsAsync(records);

        // Act
        var result = await _mockKafkaRepository.Object.GetAll();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Alias == "kafka-1");
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoRecordsExist()
    {
        // Arrange
        _mockKafkaRepository
            .Setup(repo => repo.GetAll())
            .ReturnsAsync(new List<KafkaRecord>());

        // Act
        var result = await _mockKafkaRepository.Object.GetAll();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_CallsRepository_Once()
    {
        // Arrange
        _mockKafkaRepository
            .Setup(repo => repo.GetAll())
            .ReturnsAsync(new List<KafkaRecord>());

        // Act
        await _mockKafkaRepository.Object.GetAll();

        // Assert
        _mockKafkaRepository.Verify(repo => repo.GetAll(), Times.Once);
    }
}
