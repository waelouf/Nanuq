using System.ComponentModel.DataAnnotations.Schema;

namespace Nanuq.Common.Records;

[Table("activity_type")]
public class ActivityType
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public string Color { get; set; }
	public string Icon { get; set; }
}
