using Nanuq.Sqlite.Requests;

namespace Nanuq.Sqlite.Records;

public record KafkaRecord(int Id, string BootstrapServer, string Alias)
{
	public KafkaRecord(string bootstrapServer, string alias) : 
        this(0, bootstrapServer, alias)
	{
	}

    public KafkaRecord(AddKafkaRequest addKafkaRequest) : 
        this(0, addKafkaRequest.BootstrapServer, addKafkaRequest.Alias)
    {
    }

	public KafkaRecord(UpdateKafkaRequest updateKafkaRequest) : 
        this(updateKafkaRequest.Id, updateKafkaRequest.BootstrapServer, updateKafkaRequest.Alias)
	{
	}
}

public static class KafkaRecordMapper
{
    public static KafkaRecord CreateKafkaRecord(dynamic row)
    {
        return new KafkaRecord((int)row.id, row.bootstrap_server, row.alias);
    }
}