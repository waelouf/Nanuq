using System.ComponentModel.DataAnnotations.Schema;

namespace Nanuq.Common.Records;

public class RedisRecord
{
    public RedisRecord(int id, string serverUrl, string alias, string environment)
    {
        this.Id = id;
        this.ServerUrl = serverUrl;
        this.Alias = alias;
        this.Environment = environment;
    }

    public RedisRecord(string serverUrl, string alias) : this(0, serverUrl, alias, "Development")
    {

    }

    public RedisRecord(string serverUrl, string alias, string environment) : this(0, serverUrl, alias, environment)
    {

    }

    public int Id { get; set; }

    [Column("server_url")]
    public string ServerUrl { get; set; }

    public string Alias { get; set; }

    [Column("Environment")]
    public string Environment { get; set; }
}
