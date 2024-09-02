using Microsoft.Extensions.Logging;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.EF;

namespace Nanuq.Common.Audit;

public class AuditLogRepository : IAuditLogRepository
{
	private ILogger<AuditLogRepository> logger;

	private NanuqContext context;

	public AuditLogRepository(ILogger<AuditLogRepository> logger, NanuqContext context)
	{
		this.logger = logger;
		this.context = context;
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
