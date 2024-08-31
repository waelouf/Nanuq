namespace Nanuq.Redis.Requests;

public class SetStringCacheRequest
{
	public string ServerUrl { get; set; }
	
	public int Database { get; set; }

	public string Key { get; set; }
	
	public string Value { get; set; }
	
	public double? TtlMilliseconds { get; set; }
}
