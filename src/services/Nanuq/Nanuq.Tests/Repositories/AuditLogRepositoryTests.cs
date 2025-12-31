using Xunit;
using FluentAssertions;
using Nanuq.Common.Repositories;
using Nanuq.EF;
using Microsoft.EntityFrameworkCore;

namespace Nanuq.Tests.Repositories;

public class ActivityLogRepositoryTests : IDisposable
{
    private readonly NanuqContext _context;
    private readonly ActivityLogRepository _repository;

    public ActivityLogRepositoryTests()
    {
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<NanuqContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new NanuqContext(options);
        _repository = new ActivityLogRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoActivityLogs()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ShouldAddActivityLog()
    {
        // Arrange
        var activityLog = new Common.Records.ActivityLog
        {
            Log = "Test Log",
            Details = "Test Details",
            CreatedDate = DateTime.UtcNow
        };

        // Act
        await _repository.AddAsync(activityLog);
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Log.Should().Be("Test Log");
        result.First().Details.Should().Be("Test Details");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnActivityLog_WhenExists()
    {
        // Arrange
        var activityLog = new Common.Records.ActivityLog
        {
            Log = "Test Log",
            Details = "Test Details",
            CreatedDate = DateTime.UtcNow
        };
        await _repository.AddAsync(activityLog);

        // Act
        var result = await _repository.GetByIdAsync(activityLog.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Log.Should().Be("Test Log");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
