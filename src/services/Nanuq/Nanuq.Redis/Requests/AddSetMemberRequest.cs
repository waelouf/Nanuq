namespace Nanuq.Redis.Requests;

public class AddSetMemberRequest
{
	public string ServerUrl { get; set; }

	public int Database { get; set; }

	public string Key { get; set; }

	public string Member { get; set; }
}
