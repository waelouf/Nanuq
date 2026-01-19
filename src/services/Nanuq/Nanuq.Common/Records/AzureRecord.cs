using System.ComponentModel.DataAnnotations.Schema;

namespace Nanuq.Common.Records;

[Table("azure_servers")]
public class AzureRecord
{
    public int Id { get; set; }
    public string Alias { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty; // Service Bus namespace (e.g., myapp.servicebus.windows.net)
    public string Region { get; set; } = string.Empty;
    public string Environment { get; set; } = "Development";
    public string ServiceType { get; set; } = "ServiceBus"; // ServiceBus, Storage, EventHubs, etc.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
