using Dapper;
using Microsoft.Extensions.Logging;
using Nanuq.Sqlite.Interfaces;
using Nanuq.Sqlite.Records;

namespace Nanuq.Sqlite.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
	private IDbContext dbContext;

	private ILogger<ActivityLogRepository> logger;

	public ActivityLogRepository(IDbContext context, ILogger<ActivityLogRepository> logger)
	{
		dbContext = context;
		this.logger = logger;
	}

	public async Task<IEnumerable<ActivityType>> GetAllActivityTypes()
	{
		var query = "SELECT id, name, description, color, icon FROM activity_types";
		
		try
		{			
			using var conn = dbContext.CreateConnection();
			var rows = await conn.QueryAsync(query);
			return rows.Select(row => (ActivityType)ActivityTypeMapper.CreateActivityTypeRecord(row));
		}
		catch (Exception ex)
		{
			logger.LogError(ex.Message, ex);
			throw;
		}
	}

	public async Task<IEnumerable<ActivityLog>> GetAllActivityLogs()
	{
		var query = "SELECT id, timestamp, activity_type_id, log, details FROM activity_log";

		try
		{
			using var conn = dbContext.CreateConnection();
			var rows = await conn.QueryAsync(query);
			return rows.Select(row => (ActivityLog)ActivityLogMapper.CreateActivityLogRecord(row));
		}
		catch (Exception ex)
		{
			logger.LogError(ex.Message, ex);
			throw;
		}
	}
}
