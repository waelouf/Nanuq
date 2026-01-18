using System.ComponentModel.DataAnnotations.Schema;

namespace Nanuq.Common.Records;

[Table("rabbit_mq")]
public class RabbitMQRecord
{
	public RabbitMQRecord(int id, string serverUrl, string alias, string environment)
	{
		Id = id;
		ServerUrl = serverUrl;
		Alias = alias;
		Environment = environment;
	}

	public RabbitMQRecord(string serverUrl, string alias) :
		this(0, serverUrl, alias, "Development")
	{

	}

	public RabbitMQRecord(string serverUrl, string alias, string environment) :
		this(0, serverUrl, alias, environment)
	{

	}

	public int Id { get; set; }

    [Column("server_url")]
    public string ServerUrl { get; set; }

    public string Alias { get; set; }

	[Column("Environment")]
	public string Environment { get; set; }
}
