using Microsoft.EntityFrameworkCore;
using Nanuq.Common.Records;

namespace Nanuq.EF
{
	public class NanuqContext : DbContext
	{
		public DbSet<KafkaRecord> Kafka { get; set; }

        public DbSet<ActivityType> ActivityTypes { get; set; }

		public DbSet<ActivityLog> ActivityLogs { get; set; }

		public DbSet<RedisRecord> Redis { get; set; }

        public DbSet<RabbitMQRecord> RabbitMQ { get; set; }

        public NanuqContext(DbContextOptions<NanuqContext> options)
            :base(options)
        {
            
        }
    }
}
