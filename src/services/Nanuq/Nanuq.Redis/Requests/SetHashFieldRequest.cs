namespace Nanuq.Redis.Requests;

public class SetHashFieldRequest
{
	public string ServerUrl { get; set; }

	public int Database { get; set; }

	public string Key { get; set; }

	public string Field { get; set; }

	public string Value { get; set; }
}
