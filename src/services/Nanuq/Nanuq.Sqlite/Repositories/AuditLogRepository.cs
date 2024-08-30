using Microsoft.Extensions.Logging;
using Nanuq.Common.Enums;
using Nanuq.Common.Records;
using Nanuq.EF;

namespace Nanuq.Common.Audit;

public class AuditLogRepository : IAuditLogRepository
{
	private ILogger<AuditLogRepository> logger;

	private NanuqContext context;

	public AuditLogRepository(ILogger<AuditLogRepository> logger)
	{
		this.logger = logger;
		context = new NanuqContext();
	}

	public async Task<int> Audit(ActivityTypeEnum activityType, string log, string details = "")
	{
		var activityLog = new ActivityLog
		{
			ActivityTypeId = (int)activityType,
			Details = details,
			Log = log,
			Timestamp = DateTime.UtcNow,
		};
		
		context.ActivityLogs.Add(activityLog);
		await context.SaveChangesAsync();

		return activityLog.Id;
	}
}
