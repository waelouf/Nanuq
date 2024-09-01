namespace Nanuq.Redis.Entities;

public class ServerDetails
{
    public ServerDetails()
    {
		Server = new Server();   
		Clients = new Clients();
		Memory = new Memory();
		Stats = new Stats();
		Replication = new Replication();
		CPU = new CPU();
		Databases = new List<KeyspaceDb>();
	}

    public int DatabaseCount { get; set; }

    public Server Server { get; set; }

    public Clients Clients { get; set; }

    public Memory Memory { get; set; }

    public Stats Stats { get; set; }

    public Replication Replication { get; set; }

    public CPU CPU { get; set; }

    public List<KeyspaceDb> Databases { get; set; }
}

public class Server
{
	public string RedisVersion { get; set; }
	public string RedisMode { get; set; }
	public string OS { get; set; }
	public string TcpPort { get; set; }
	public string UptimeInDays { get; set; }
	public string Executable { get; set; }
}

public class Clients
{
	public int ConnectedClients { get; set; }
	public int BlockedClients { get; set; }
}

public class Memory
{
	public string UsedMemoryHuman { get; set; }
	public string TotalSystemMemoryHuman { get; set; }
}

public class Stats
{
	public int TotalConnectionsReceived { get; set; }
	public int TotalCommandsProcessed { get; set; }
	public long TotalNetInputBytes { get; set; }
	public long TotalNetOutputBytes { get; set; }
	public int KeyspaceHits { get; set; }
	public int KeyspaceMisses { get; set; }
	public int PubsubChannels { get; set; }
	public int PubsubshardChannels { get; set; }
	public int TotalErrorReplies { get; set; }
	public int TotalReadsProcessed { get; set; }
	public int TotalWritesProcessed { get; set; }
}

public class Replication
{
	public string Role { get; set; }
	public int ConnectedSlaves { get; set; }
	public string MasterFailoverState { get; set; }
}

public class CPU
{
	public double UsedCpuSys { get; set; }
	public double UsedCpuUser { get; set; }
}

public class KeyspaceDb
{
	public string Database { get; set; }
	public int Keys { get; set; }
	public int Expires { get; set; }
	public int AvgTtl { get; set; }
}
