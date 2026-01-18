using Nanuq.Common.Requests;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nanuq.Common.Records;

public class KafkaRecord
{
	[Column("id")]
	public int Id { get; set; }

	[Column("bootstrap_server")]
	public string BootstrapServer { get; set; }

	[Column("alias")]
	public string Alias { get; set; }

	[Column("Environment")]
	public string Environment { get; set; }

	public KafkaRecord(string bootstrapServer, string alias) :
        this(0, bootstrapServer, alias, "Development")
	{
	}

	public KafkaRecord(string bootstrapServer, string alias, string environment) :
        this(0, bootstrapServer, alias, environment)
	{
	}

	public KafkaRecord(int id, string bootstrapServer, string alias, string environment)
	{
		Id = id;
		BootstrapServer = bootstrapServer;
		Alias = alias;
		Environment = environment;
	}
}