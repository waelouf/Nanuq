namespace Nanuq.Sqlite.Records;

public class KafkaRecord
{
    public KafkaRecord()
    {
        
    }

	public int Id { get; set; }
	public string BootstrapServer { get; set; }
	public string Alias { get; set; } 
}
