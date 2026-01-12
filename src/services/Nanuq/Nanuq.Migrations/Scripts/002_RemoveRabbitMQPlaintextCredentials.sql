-- Remove plaintext credentials from rabbit_mq table
-- Credentials will now be stored encrypted in ServerCredentials table

-- Check if columns exist before dropping them (SQLite doesn't support ALTER TABLE DROP COLUMN)
-- We need to recreate the table without the Username and Password columns

-- Create new table without credential columns
CREATE TABLE IF NOT EXISTS rabbit_mq_new (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    server_url TEXT NOT NULL,
    alias TEXT NOT NULL
);

-- Copy data from old table (excluding username and password)
INSERT INTO rabbit_mq_new (id, server_url, alias)
SELECT id, server_url, alias
FROM rabbit_mq
WHERE EXISTS (SELECT 1 FROM rabbit_mq);

-- Drop old table
DROP TABLE IF EXISTS rabbit_mq;

-- Rename new table to original name
ALTER TABLE rabbit_mq_new RENAME TO rabbit_mq;
