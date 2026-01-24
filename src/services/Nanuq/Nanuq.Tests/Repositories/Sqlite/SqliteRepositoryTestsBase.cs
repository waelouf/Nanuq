using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Nanuq.Common.Interfaces;
using Nanuq.EF;
using Nanuq.Tests.Helpers;

namespace Nanuq.Tests.Repositories.Sqlite;

/// <summary>
/// Base class for SQLite repository tests to reduce code duplication
/// </summary>
/// <typeparam name="TRepository">The repository type being tested</typeparam>
public abstract class SqliteRepositoryTestsBase<TRepository> : IDisposable
{
    protected NanuqContext Context { get; }
    protected Mock<ILogger<TRepository>> MockLogger { get; }
    protected Mock<IAuditLogRepository> MockAuditLog { get; }

    protected SqliteRepositoryTestsBase()
    {
        Context = TestDbContextFactory.CreateInMemoryContext();
        MockLogger = new Mock<ILogger<TRepository>>();
        MockAuditLog = new Mock<IAuditLogRepository>();

        // Setup default audit log behavior to return a task ID
        MockAuditLog
            .Setup(x => x.Audit(It.IsAny<Common.Enums.ActivityTypeEnum>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(1);
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
