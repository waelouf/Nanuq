namespace Nanuq.Sqlite.Records;

public record ActivityLog(int Id, DateTime Timestamp, int ActivityTypeId, string Log, string Details);

public static class ActivityLogMapper
{
	public static ActivityLog CreateActivityLogRecord(dynamic row)
	{
		return new ActivityLog((int)row.id, row.timestamp, row.activity_type_id, row.log, row.details);
	}
}