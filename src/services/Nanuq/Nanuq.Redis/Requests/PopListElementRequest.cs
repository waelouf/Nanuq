namespace Nanuq.Redis.Requests;

public class PopListElementRequest
{
	public string ServerUrl { get; set; }

	public int Database { get; set; }

	public string Key { get; set; }

	public bool PopLeft { get; set; }
}
