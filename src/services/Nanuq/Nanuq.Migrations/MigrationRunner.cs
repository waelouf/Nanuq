using DbUp;
using Microsoft.Extensions.Logging;

namespace Nanuq.Migrations;

public class MigrationRunner
{
    private readonly string _connectionString;
    private readonly ILogger<MigrationRunner>? _logger;

    public MigrationRunner(string connectionString, ILogger<MigrationRunner>? logger = null)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger;
    }

    /// <summary>
    /// Executes all pending database migrations
    /// </summary>
    /// <returns>True if migrations succeeded, false if they failed</returns>
    public bool Run()
    {
        _logger?.LogInformation("Starting database migration...");

        var upgrader = DeployChanges.To
            .SQLiteDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(MigrationRunner).Assembly)
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            _logger?.LogError(result.Error, "Database migration failed");
            return false;
        }

        _logger?.LogInformation("Database migration completed successfully");
        return true;
    }

    /// <summary>
    /// Checks if there are any pending migrations
    /// </summary>
    /// <returns>True if migrations are needed, false otherwise</returns>
    public bool IsUpgradeRequired()
    {
        var upgrader = DeployChanges.To
            .SQLiteDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(MigrationRunner).Assembly)
            .Build();

        return upgrader.IsUpgradeRequired();
    }
}
