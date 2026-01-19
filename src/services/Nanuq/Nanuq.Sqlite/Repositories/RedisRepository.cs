using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nanuq.Sqlite.Repositories;

public class RedisRepository : IRedisRepository
{
	private ILogger<RedisRepository> logger;

	private NanuqContext dbContext;
	private IAuditLogRepository auditLog;

	public RedisRepository(NanuqContext dbContext, ILogger<RedisRepository> logger, IAuditLogRepository auditLog)
	{
		this.dbContext = dbContext;
		this.logger = logger;
		this.auditLog = auditLog;
	}

	public async Task<int> Add(RedisRecord record)
	{
		dbContext.Redis.Add(record);
		await dbContext.SaveChangesAsync();

		// ADD audit logging
		var details = System.Text.Json.JsonSerializer.Serialize(new
		{
			serverId = record.Id,
			serverUrl = record.ServerUrl,
			alias = record.Alias,
			environment = record.Environment
		});
		await auditLog.Audit(ActivityTypeEnum.AddRedisServer,
			$"Redis server '{record.Alias}' ({record.ServerUrl}) added to {record.Environment} environment",
			details);

		return record.Id;
	}


	public async Task<bool> Delete(int id)
	{
		var record = await dbContext.Redis.FindAsync(id);
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
			await auditLog.Audit(ActivityTypeEnum.RemoveRedisServer,
				$"Redis server '{record.Alias}' ({record.ServerUrl}) removed from {record.Environment} environment",
				details);

			dbContext.Redis.Remove(record);
			dbContext.SaveChanges();
			return true;
		}

		return false;
	}

	public async Task<RedisRecord> Get(int id)
	{
		var record = await dbContext.Redis.FindAsync(id);
		return record;
	}

	public async Task<IEnumerable<RedisRecord>> GetAll()
	{
		return await dbContext.Redis.ToListAsync();
	}
}
