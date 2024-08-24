namespace Nanuq.Sqlite.Records;

public record ActivityType(int Id, string Name, string Description, string Color, string Icon);

public static class ActivityTypeMapper
{
	public static ActivityType CreateActivityTypeRecord(dynamic row)
	{
		return new ActivityType((int)row.id, row.name, row.description, row.color, row.icon);
	}
}