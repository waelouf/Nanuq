using System.ComponentModel.DataAnnotations.Schema;

namespace Nanuq.Common.Records;

[Table("rabbit_mq")]
public class RabbitMQRecord
{
	public RabbitMQRecord(int id, string serverUrl, string alias)
	{
		Id = id;
		ServerUrl = serverUrl;
		Alias = alias;
	}
	public RabbitMQRecord(string serverUrl, string alias) :
		this(0, serverUrl, alias)
	{

	}

	public int Id { get; set; }

    [Column("server_url")]
    public string ServerUrl { get; set; }

    public string Alias { get; set; }
}
