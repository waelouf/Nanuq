using Xunit;
using FluentAssertions;
using Nanuq.Sqlite.Repositories;
using Nanuq.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Nanuq.Common.Records;

namespace Nanuq.Tests.Repositories;

public class ActivityLogRepositoryTests : IDisposable
{
    private readonly NanuqContext _context;
    private readonly ActivityLogRepository _repository;
    private readonly Mock<ILogger<ActivityLogRepository>> _mockLogger;

    public ActivityLogRepositoryTests()
    {
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<NanuqContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new NanuqContext(options);
        _mockLogger = new Mock<ILogger<ActivityLogRepository>>();
        _repository = new ActivityLogRepository(_mockLogger.Object, _context);
    }

    [Fact]
    public async Task GetAllActivityLogs_ShouldReturnEmptyList_WhenNoActivityLogs()
    {
        // Act
        var result = await _repository.GetAllActivityLogs();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllActivityLogs_ShouldReturnActivityLogs_WhenLogsExist()
    {
        // Arrange
        var activityLog = new ActivityLog
        {
            Log = "Test Log",
            Details = "Test Details",
            Timestamp = DateTime.UtcNow,
            ActivityTypeId = 1
        };

        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllActivityLogs();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Log.Should().Be("Test Log");
        result.First().Details.Should().Be("Test Details");
    }

    [Fact]
    public async Task GetAllActivityTypes_ShouldReturnEmptyList_WhenNoTypes()
    {
        // Act
        var result = await _repository.GetAllActivityTypes();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllActivityTypes_ShouldReturnTypes_WhenTypesExist()
    {
        // Arrange
        var activityType = new ActivityType
        {
            Name = "Test Type",
            Description = "Test Description",
            Color = "#FF0000",
            Icon = "test-icon"
        };

        _context.ActivityTypes.Add(activityType);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllActivityTypes();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Test Type");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
