using Nanuq.Common.Records;

namespace Nanuq.Common.Requests;

public record AddKafkaRecordRequest(string BootstrapServer, string Alias, string Environment = "Development");

public static partial class Extension
{
    public static KafkaRecord ToRecord(this AddKafkaRecordRequest request)
    {
        return new KafkaRecord(request.BootstrapServer, request.Alias, request.Environment);
    }
}
