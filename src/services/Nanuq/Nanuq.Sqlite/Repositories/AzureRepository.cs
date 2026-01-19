using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.EF;

namespace Nanuq.Sqlite.Repositories;

public interface IAzureRepository
{
    Task<int> Add(AzureRecord record);
    Task<AzureRecord?> Get(int id);
    Task<IEnumerable<AzureRecord>> GetAll();
    Task<bool> Update(AzureRecord record);
    Task<bool> Delete(int id);
}

public class AzureRepository : IAzureRepository
{
    private readonly ILogger<AzureRepository> logger;
    private readonly NanuqContext dbContext;
    private readonly IAuditLogRepository auditLog;

    public AzureRepository(
        ILogger<AzureRepository> logger,
        NanuqContext dbContext,
        IAuditLogRepository auditLog)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.auditLog = auditLog;
    }

    public async Task<int> Add(AzureRecord record)
    {
        record.CreatedAt = DateTime.UtcNow;
        record.UpdatedAt = DateTime.UtcNow;

        dbContext.AzureServers.Add(record);
        await dbContext.SaveChangesAsync();

        // Audit logging
        var details = System.Text.Json.JsonSerializer.Serialize(new
        {
            serverId = record.Id,
            azureNamespace = record.Namespace,
            alias = record.Alias,
            region = record.Region,
            environment = record.Environment,
            serviceType = record.ServiceType
        });

        await auditLog.Audit(
            ActivityTypeEnum.AddAzureServer,
            $"Azure server '{record.Alias}' ({record.Namespace}) added to {record.Environment} environment",
            details
        );

        logger.LogInformation($"Added Azure server: {record.Alias}");
        return record.Id;
    }

    public async Task<AzureRecord?> Get(int id)
    {
        return await dbContext.AzureServers.FindAsync(id);
    }

    public async Task<IEnumerable<AzureRecord>> GetAll()
    {
        return await dbContext.AzureServers.ToListAsync();
    }

    public async Task<bool> Update(AzureRecord record)
    {
        var existing = await dbContext.AzureServers.FindAsync(record.Id);
        if (existing == null) return false;

        existing.Alias = record.Alias;
        existing.Namespace = record.Namespace;
        existing.Region = record.Region;
        existing.Environment = record.Environment;
        existing.ServiceType = record.ServiceType;
        existing.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        logger.LogInformation($"Updated Azure server: {record.Alias}");
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        var record = await dbContext.AzureServers.FindAsync(id);
        if (record == null) return false;

        // Audit log BEFORE deletion
        var details = System.Text.Json.JsonSerializer.Serialize(new
        {
            serverId = record.Id,
            azureNamespace = record.Namespace,
            alias = record.Alias,
            region = record.Region,
            environment = record.Environment,
            serviceType = record.ServiceType
        });

        await auditLog.Audit(
            ActivityTypeEnum.RemoveAzureServer,
            $"Azure server '{record.Alias}' ({record.Namespace}) removed from {record.Environment} environment",
            details
        );

        dbContext.AzureServers.Remove(record);
        await dbContext.SaveChangesAsync();

        logger.LogInformation($"Deleted Azure server: {record.Alias}");
        return true;
    }
}
