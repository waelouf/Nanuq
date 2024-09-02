using Nanuq.Common.Enums;

namespace Nanuq.Common.Interfaces;

public interface IAuditLogRepository
{
    Task<int> Audit(ActivityTypeEnum activityType, string log, string details = "");
}
