using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.EF;

namespace Nanuq.Sqlite.Repositories;

/// <summary>
/// Repository for managing AWS server configurations in SQLite
/// </summary>
public class AwsRepository : IAwsRepository
{
    private ILogger<AwsRepository> logger;
    private NanuqContext dbContext;
    private IAuditLogRepository auditLog;

    public AwsRepository(ILogger<AwsRepository> logger, NanuqContext dbContext, IAuditLogRepository auditLog)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.auditLog = auditLog;
    }

    public async Task<int> Add(AwsRecord record)
    {
        dbContext.AWS.Add(record);
        await dbContext.SaveChangesAsync();

        // Audit logging
        var details = System.Text.Json.JsonSerializer.Serialize(new
        {
            serverId = record.Id,
            region = record.Region,
            alias = record.Alias,
            environment = record.Environment,
            serviceType = record.ServiceType
        });

        await auditLog.Audit(ActivityTypeEnum.AddAWSServer,
            $"AWS {record.ServiceType} server '{record.Alias}' added to {record.Environment} environment (region: {record.Region})",
            details);

        return record.Id;
    }

    public async Task<bool> Delete(int id)
    {
        var record = await dbContext.AWS.FindAsync(id);
        if (record != null)
        {
            // Audit log BEFORE deletion
            var details = System.Text.Json.JsonSerializer.Serialize(new
            {
                serverId = record.Id,
                region = record.Region,
                alias = record.Alias,
                environment = record.Environment,
                serviceType = record.ServiceType
            });

            await auditLog.Audit(ActivityTypeEnum.RemoveAWSServer,
                $"AWS {record.ServiceType} server '{record.Alias}' removed from {record.Environment} environment (region: {record.Region})",
                details);

            dbContext.AWS.Remove(record);
            dbContext.SaveChanges();
            return true;
        }

        return false;
    }

    public async Task<AwsRecord> Get(int id)
    {
        return await dbContext.AWS.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<AwsRecord>> GetAll()
    {
        return await dbContext.AWS.AsNoTracking().ToListAsync();
    }

    public async Task<bool> Update(AwsRecord record)
    {
        var existing = await dbContext.AWS.FindAsync(record.Id);
        if (existing != null)
        {
            existing.Region = record.Region;
            existing.Alias = record.Alias;
            existing.Environment = record.Environment;
            existing.ServiceType = record.ServiceType;

            await dbContext.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
