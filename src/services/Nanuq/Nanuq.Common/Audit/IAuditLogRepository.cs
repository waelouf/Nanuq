using Nanuq.Common.Enums;

namespace Nanuq.Common.Audit;

public interface IAuditLogRepository
{
	Task<int> Audit(ActivityTypeEnum activityType, string log, string details = "");
}
