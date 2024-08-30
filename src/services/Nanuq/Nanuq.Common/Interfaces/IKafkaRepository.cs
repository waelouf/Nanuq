using Nanuq.Common.Records;

namespace Nanuq.Common.Interfaces;

public interface IKafkaRepository
{
	public Task<IEnumerable<KafkaRecord>> GetAll();

	public Task<KafkaRecord> Get(int id);
	
	public Task<int> Add(KafkaRecord record);

	public Task<bool> Delete(int id);

	public Task<bool> Update(KafkaRecord record);
}
