using Nanuq.Sqlite.Records;

namespace Nanuq.Sqlite.Interfaces;

public interface IKafkaRepository
{
	public Task<IEnumerable<KafkaRecord>> GetAll();

	public Task<KafkaRecord> Get(int id);
	
	public Task Add(KafkaRecord record);

	public Task<bool> Delete(int id);

	public Task Update(KafkaRecord record);
}
