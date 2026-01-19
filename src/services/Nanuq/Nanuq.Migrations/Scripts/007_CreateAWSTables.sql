-- Create AWS servers table
CREATE TABLE IF NOT EXISTS aws_servers (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    region TEXT NOT NULL,
    alias TEXT NOT NULL,
    Environment TEXT NOT NULL DEFAULT 'Development',
    service_type TEXT NOT NULL DEFAULT 'SQS'
);

-- Create index on region for filtering
CREATE INDEX IF NOT EXISTS IX_AWSServers_Region
    ON aws_servers(region);

-- Create index on service_type for filtering
CREATE INDEX IF NOT EXISTS IX_AWSServers_ServiceType
    ON aws_servers(service_type);
