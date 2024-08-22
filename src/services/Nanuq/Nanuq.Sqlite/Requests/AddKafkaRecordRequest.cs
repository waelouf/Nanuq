using Nanuq.Sqlite.Records;

namespace Nanuq.Sqlite.Requests;

public class AddKafkaRecordRequest
{
    public string BootstrapServer { get; set; }

    public string Alias { get; set; }
}

public static partial class Extension
{
    public static KafkaRecord ToRecord(this AddKafkaRecordRequest request)
    {
        return new KafkaRecord(request.BootstrapServer, request.Alias);
    }
}
