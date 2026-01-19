using System.ComponentModel.DataAnnotations.Schema;

namespace Nanuq.Common.Records;

/// <summary>
/// Represents an AWS server configuration
/// </summary>
[Table("aws_servers")]
public class AwsRecord
{
    public int Id { get; set; }

    [Column("region")]
    public string Region { get; set; }  // us-east-1, eu-west-1, etc.

    public string Alias { get; set; }

    [Column("Environment")]
    public string Environment { get; set; }

    [Column("service_type")]
    public string ServiceType { get; set; }  // "SQS", "SNS", "S3", etc. - for future extensibility

    public AwsRecord()
    {
        Region = string.Empty;
        Alias = string.Empty;
        Environment = "Development";
        ServiceType = "SQS";
    }

    public AwsRecord(string region, string alias, string environment, string serviceType = "SQS")
    {
        Region = region;
        Alias = alias;
        Environment = environment;
        ServiceType = serviceType;
    }
}
