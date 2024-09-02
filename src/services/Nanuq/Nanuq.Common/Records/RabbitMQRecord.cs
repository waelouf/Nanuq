using System.ComponentModel.DataAnnotations.Schema;

namespace Nanuq.Common.Records;

[Table("rabbit_mq")]
public class RabbitMQRecord
{
	public RabbitMQRecord(int id, string serverUrl, string username, string password, string alias)
	{
		Id = id;
		ServerUrl = serverUrl;
		Username = username;
		Password = password;
		Alias = alias;
	}
	public RabbitMQRecord(string serverUrl, string username, string password, string alias) :
		this(0, serverUrl, username, password, alias) 
	{
		
	}

	public int Id { get; set; }

    [Column("server_url")]
    public string ServerUrl { get; set; }

	public string Username { get; set; }

    public string Password { get; set; }

    public string Alias { get; set; }
}
