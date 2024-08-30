using System.ComponentModel.DataAnnotations.Schema;

namespace Nanuq.Common.Records;

[Table("activity_log")]
public class ActivityLog
{
	public int Id { get; set; }

	[Column("timestamp")]
	public DateTime Timestamp { get; set; }

	[Column("activity_type_id")]
	public int ActivityTypeId { get; set; } 

	public string Log { get; set; } 
	public string Details { get; set; }
}
