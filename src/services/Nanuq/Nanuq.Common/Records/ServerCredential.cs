using System.ComponentModel.DataAnnotations.Schema;

namespace Nanuq.Common.Records;

public class ServerCredential
{
    [Column("Id")]
    public int Id { get; set; }

    [Column("ServerId")]
    public int ServerId { get; set; }

    [Column("ServerType")]
    public string ServerType { get; set; } = string.Empty;

    [Column("Username")]
    public string? Username { get; set; } // Encrypted

    [Column("Password")]
    public string? Password { get; set; } // Encrypted

    [Column("AdditionalConfig")]
    public string? AdditionalConfig { get; set; } // Encrypted JSON

    [Column("EncryptionKeyId")]
    public string EncryptionKeyId { get; set; } = string.Empty;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    [Column("LastUsedAt")]
    public DateTime? LastUsedAt { get; set; }
}
