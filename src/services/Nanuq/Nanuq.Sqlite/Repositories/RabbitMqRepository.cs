using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.EF;

namespace Nanuq.Sqlite.Repositories;

public class RabbitMqRepository : IRabbitMqRepository
{
	private ILogger<RabbitMqRepository> logger;

	private NanuqContext dbContext;
	private IAuditLogRepository auditLog;

	public RabbitMqRepository(ILogger<RabbitMqRepository> logger, NanuqContext dbContext, IAuditLogRepository auditLog)
	{
		this.logger = logger;
		this.dbContext = dbContext;
		this.auditLog = auditLog;
	}

	public async Task<int> Add(RabbitMQRecord record)
	{
		dbContext.RabbitMQ.Add(record);
		await dbContext.SaveChangesAsync();

		// ADD audit logging
		var details = System.Text.Json.JsonSerializer.Serialize(new
		{
			serverId = record.Id,
			serverUrl = record.ServerUrl,
			alias = record.Alias,
			environment = record.Environment
		});
		await auditLog.Audit(ActivityTypeEnum.AddRabbitMQServer,
			$"RabbitMQ server '{record.Alias}' ({record.ServerUrl}) added to {record.Environment} environment",
			details);

		return record.Id;
	}

	public async Task<bool> Delete(int id)
	{
		var record = await dbContext.RabbitMQ.FindAsync(id);
		if (record != null)
		{
			// ADD audit log BEFORE deletion
			var details = System.Text.Json.JsonSerializer.Serialize(new
			{
				serverId = record.Id,
				serverUrl = record.ServerUrl,
				alias = record.Alias,
				environment = record.Environment
			});
			await auditLog.Audit(ActivityTypeEnum.RemoveRabbitMQServer,
				$"RabbitMQ server '{record.Alias}' ({record.ServerUrl}) removed from {record.Environment} environment",
				details);

			dbContext.RabbitMQ.Remove(record);
			dbContext.SaveChanges();
			return true;
		}

		return false;
	}

	public async Task<RabbitMQRecord> Get(int id)
	{
		var record = await dbContext.RabbitMQ.FindAsync(id);
		return record;
	}

	public async Task<IEnumerable<RabbitMQRecord>> GetAll()
	{
		return await dbContext.RabbitMQ.ToListAsync();
	}
}
