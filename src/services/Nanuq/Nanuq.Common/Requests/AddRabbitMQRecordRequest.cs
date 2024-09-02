using Nanuq.Common.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nanuq.Common.Requests;

public record AddRabbitMQRecordRequest(string ServerUrl, string Username, string Password, string Alias);

public static partial class Extension
{
	public static RabbitMQRecord ToRecord(this AddRabbitMQRecordRequest request)
	{
		return new RabbitMQRecord(request.ServerUrl, request.Username, request.Password, request.Alias);
	}
}