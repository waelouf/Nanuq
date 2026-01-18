namespace Nanuq.Redis.Requests;

public class AddStreamEntryRequest
{
	public string ServerUrl { get; set; }

	public int Database { get; set; }

	public string Key { get; set; }

	public Dictionary<string, string> Fields { get; set; }
}
