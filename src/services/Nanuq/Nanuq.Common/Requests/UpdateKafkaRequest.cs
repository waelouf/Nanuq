using Nanuq.Common.Records;

namespace Nanuq.Common.Requests;

public record UpdateKafkaRequest(int Id, string BootstrapServer, string Alias);

public static partial class Extension
{
	public static KafkaRecord ToRecord(this UpdateKafkaRequest request)
	{
		return new KafkaRecord(request.Id ,request.BootstrapServer, request.Alias);
	}
}