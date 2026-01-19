-- Create azure_servers table
CREATE TABLE IF NOT EXISTS azure_servers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Alias TEXT NOT NULL,
    Namespace TEXT NOT NULL,
    Region TEXT NOT NULL,
    Environment TEXT NOT NULL DEFAULT 'Development',
    ServiceType TEXT NOT NULL DEFAULT 'ServiceBus',
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'utc')),
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now', 'utc'))
);

-- Create index on Alias for faster searches
CREATE INDEX IF NOT EXISTS IX_AzureServers_Alias
    ON azure_servers(Alias);

-- Create index on Environment for filtering
CREATE INDEX IF NOT EXISTS IX_AzureServers_Environment
    ON azure_servers(Environment);
