using Nanuq.Common.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nanuq.Common.Requests;

public record AddRabbitMQRecordRequest(string ServerUrl, string Alias, string Environment = "Development");

public static partial class Extension
{
	public static RabbitMQRecord ToRecord(this AddRabbitMQRecordRequest request)
	{
		return new RabbitMQRecord(request.ServerUrl, request.Alias, request.Environment);
	}
}