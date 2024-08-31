using System.ComponentModel.DataAnnotations.Schema;

namespace Nanuq.Common.Records;

public class RedisRecord
{
    public RedisRecord(int id, string serverUrl, string alias)
    {
        this.Id = id;
        this.ServerUrl = serverUrl;
        this.Alias = alias;
    }

    public RedisRecord(string serverUrl, string alias) : this(0, serverUrl, alias)
    {
        
    }

    public int Id { get; set; }

    [Column("server_url")]
    public string ServerUrl { get; set; }

    public string Alias { get; set; }
}
