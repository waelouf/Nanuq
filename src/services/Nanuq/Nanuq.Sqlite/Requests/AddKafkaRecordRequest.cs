using Nanuq.Sqlite.Records;

namespace Nanuq.Sqlite.Requests;

public record AddKafkaRecordRequest(string BootstrapServer, string Alias);

public static partial class Extension
{
    public static KafkaRecord ToRecord(this AddKafkaRecordRequest request)
    {
        return new KafkaRecord(request.BootstrapServer, request.Alias);
    }
}
