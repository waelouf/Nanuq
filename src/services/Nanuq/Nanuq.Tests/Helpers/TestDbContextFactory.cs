using Microsoft.EntityFrameworkCore;
using Nanuq.EF;

namespace Nanuq.Tests.Helpers;

/// <summary>
/// Factory for creating in-memory database contexts for testing
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates a new NanuqContext with InMemory database provider
    /// </summary>
    /// <param name="databaseName">Optional database name. If null, generates unique GUID</param>
    /// <returns>Configured NanuqContext</returns>
    public static NanuqContext CreateInMemoryContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<NanuqContext>()
            .UseInMemoryDatabase(databaseName: databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new NanuqContext(options);
    }
}
