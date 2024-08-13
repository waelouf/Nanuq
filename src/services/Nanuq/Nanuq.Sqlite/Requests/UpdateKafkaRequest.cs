using Nanuq.Sqlite.Records;

namespace Nanuq.Sqlite.Requests;

public class UpdateKafkaRequest
{
    public int Id { get; set; }
    public string BootstrapServer { get; set; }

	public string Alias { get; set; }
}

public static partial class Extension
{
	public static KafkaRecord ToRecord(this UpdateKafkaRequest request)
	{
		return new KafkaRecord(request.Id ,request.BootstrapServer, request.Alias);
	}
}