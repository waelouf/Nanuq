-- Create ServerCredentials table for storing encrypted server authentication credentials
CREATE TABLE IF NOT EXISTS ServerCredentials (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ServerId INTEGER NOT NULL,
    ServerType TEXT NOT NULL,
    Username TEXT,
    Password TEXT,
    AdditionalConfig TEXT,
    EncryptionKeyId TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastUsedAt TEXT
);

-- Create unique index to ensure one credential per server
CREATE UNIQUE INDEX IF NOT EXISTS IX_ServerCredentials_Server
    ON ServerCredentials(ServerId, ServerType);
