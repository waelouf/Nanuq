using Microsoft.EntityFrameworkCore;
using Nanuq.Common.Records;

namespace Nanuq.EF
{
	public class NanuqContext : DbContext
	{
		public DbSet<KafkaRecord> Kafka { get; set; }

        public DbSet<ActivityType> ActivityTypes { get; set; }

		public DbSet<ActivityLog> ActivityLogs { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Data Source=Database/Nanuq.db");
		}
	}
}
