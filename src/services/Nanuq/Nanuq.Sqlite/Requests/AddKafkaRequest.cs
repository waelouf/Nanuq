using Microsoft.AspNetCore.Http;
using Nanuq.Sqlite.Records;

namespace Nanuq.Sqlite.Requests;

public class AddKafkaRequest
{
    public string BootstrapServer { get; set; }

    public string Alias { get; set; }
}

public static partial class Extension
{
    public static KafkaRecord ToRecord(this AddKafkaRequest request)
    {
        return new KafkaRecord(request.BootstrapServer, request.Alias);
    }
}
