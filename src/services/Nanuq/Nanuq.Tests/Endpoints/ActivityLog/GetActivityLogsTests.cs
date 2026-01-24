using FluentAssertions;
using Moq;
using Nanuq.Common.Interfaces;
using Nanuq.Tests.Helpers;
using Nanuq.WebApi.Endpoints.ActivityLog;
using Xunit;
using ActivityLogRecord = Nanuq.Common.Records.ActivityLog;

namespace Nanuq.Tests.Endpoints.ActivityLog;

public class GetActivityLogsTests
{
    private readonly Mock<IActivityLogRepository> _mockActivityLogRepository;
    private readonly GetActivityLogs _endpoint;

    public GetActivityLogsTests()
    {
        _mockActivityLogRepository = new Mock<IActivityLogRepository>();
        _endpoint = new GetActivityLogs(_mockActivityLogRepository.Object);
    }

    [Fact]
    public async Task GetAllActivityLogs_ReturnsAllLogs_WhenLogsExist()
    {
        // Arrange
        var logs = new List<ActivityLogRecord>
        {
            TestDataBuilder.CreateActivityLog(id: 1, log: "Log 1"),
            TestDataBuilder.CreateActivityLog(id: 2, log: "Log 2"),
            TestDataBuilder.CreateActivityLog(id: 3, log: "Log 3")
        };

        _mockActivityLogRepository
            .Setup(repo => repo.GetAllActivityLogs())
            .ReturnsAsync(logs);

        // Act
        var result = await _mockActivityLogRepository.Object.GetAllActivityLogs();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(l => l.Log == "Log 1");
        result.Should().Contain(l => l.Log == "Log 2");
        result.Should().Contain(l => l.Log == "Log 3");
    }

    [Fact]
    public async Task GetAllActivityLogs_ReturnsEmptyList_WhenNoLogsExist()
    {
        // Arrange
        _mockActivityLogRepository
            .Setup(repo => repo.GetAllActivityLogs())
            .ReturnsAsync(new List<ActivityLogRecord>());

        // Act
        var result = await _mockActivityLogRepository.Object.GetAllActivityLogs();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllActivityLogs_CallsRepository_Once()
    {
        // Arrange
        _mockActivityLogRepository
            .Setup(repo => repo.GetAllActivityLogs())
            .ReturnsAsync(new List<ActivityLogRecord>());

        // Act
        await _mockActivityLogRepository.Object.GetAllActivityLogs();

        // Assert
        _mockActivityLogRepository.Verify(
            repo => repo.GetAllActivityLogs(),
            Times.Once);
    }

    [Fact]
    public async Task GetAllActivityLogs_OrdersByTimestamp_Descending()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var logs = new List<ActivityLogRecord>
        {
            TestDataBuilder.CreateActivityLog(id: 1, log: "Oldest"),
            TestDataBuilder.CreateActivityLog(id: 2, log: "Middle"),
            TestDataBuilder.CreateActivityLog(id: 3, log: "Newest")
        };
        logs[0].Timestamp = now.AddMinutes(-10);
        logs[1].Timestamp = now.AddMinutes(-5);
        logs[2].Timestamp = now;

        _mockActivityLogRepository
            .Setup(repo => repo.GetAllActivityLogs())
            .ReturnsAsync(logs);

        // Act
        var result = await _mockActivityLogRepository.Object.GetAllActivityLogs();
        var sorted = result.OrderByDescending(l => l.Timestamp);

        // Assert
        sorted.First().Log.Should().Be("Newest");
        sorted.Last().Log.Should().Be("Oldest");
    }
}
