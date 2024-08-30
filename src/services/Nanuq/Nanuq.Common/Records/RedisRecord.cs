using System.ComponentModel.DataAnnotations.Schema;

namespace Nanuq.Common.Records;

public class RedisRecord
{
    public int Id { get; set; }

    [Column("server_url")]
    public string ServerUrl { get; set; }

    public string Alias { get; set; }
}
