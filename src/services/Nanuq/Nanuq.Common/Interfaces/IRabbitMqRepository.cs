using Nanuq.Common.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nanuq.Common.Interfaces;

public interface IRabbitMqRepository
{
	Task<int> Add(RabbitMQRecord record);

	Task<bool> Delete(int id);

	Task<RabbitMQRecord> Get(int id);

	Task<IEnumerable<RabbitMQRecord>> GetAll();
}
