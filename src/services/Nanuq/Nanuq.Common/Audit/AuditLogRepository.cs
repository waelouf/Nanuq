using Dapper;
using Microsoft.Extensions.Logging;
using Nanuq.Common.Enums;
using Nanuq.Sqlite.Interfaces;
using System.Data;

namespace Nanuq.Common.Audit;

public class AuditLogRepository : IAuditLogRepository
{
	private IDbContext dbContext;

	private ILogger<AuditLogRepository> logger;

	public AuditLogRepository(IDbContext dbContext, ILogger<AuditLogRepository> logger)
	{
		this.dbContext = dbContext;
		this.logger = logger;
	}

	public async Task<int> Audit(ActivityTypeEnum activityType, string log, string details = "")
	{
		var query = """
				insert into activity_log(timestamp, activity_type_id, log, details)
				values(@timestamp, @activity_type_id, @log, @details)
				""";
		using var conn = dbContext.CreateConnection();
		await conn.ExecuteAsync(query,
			new { timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.FFFFFFF"), activity_type_id = (int)activityType, log, details },
			commandType: CommandType.Text);
		query = "SELECT last_insert_rowid()";
		var insertedId = await conn.QueryAsync<int>(query);
		return insertedId.FirstOrDefault();
	}
}
