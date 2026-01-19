-- Create activity_type table
CREATE TABLE IF NOT EXISTS activity_type (
    Id INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT NOT NULL,
    Color TEXT NOT NULL,
    Icon TEXT NOT NULL
);

-- Create activity_log table
CREATE TABLE IF NOT EXISTS activity_log (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp TEXT NOT NULL DEFAULT (datetime('now', 'utc')),
    activity_type_id INTEGER NOT NULL,
    Log TEXT NOT NULL,
    Details TEXT,
    FOREIGN KEY (activity_type_id) REFERENCES activity_type(Id)
);

-- Create index on timestamp for faster queries
CREATE INDEX IF NOT EXISTS IX_ActivityLog_Timestamp
    ON activity_log(timestamp DESC);

-- Create index on activity_type_id for filtering
CREATE INDEX IF NOT EXISTS IX_ActivityLog_ActivityTypeId
    ON activity_log(activity_type_id);

-- Seed activity_type data (14 types)
INSERT OR REPLACE INTO activity_type (Id, Name, Description, Color, Icon) VALUES
(1, 'Add Kafka Server', 'Kafka server was added to configuration', '#1976D2', 'mdi-server-plus'),
(2, 'Remove Kafka Server', 'Kafka server was removed from configuration', '#D32F2F', 'mdi-server-minus'),
(3, 'Add Kafka Topic', 'Kafka topic was created', '#4CAF50', 'mdi-message-plus'),
(4, 'Remove Kafka Topic', 'Kafka topic was deleted', '#FF5722', 'mdi-message-minus'),
(5, 'Add Redis Cache', 'Redis cache entry was added', '#43A047', 'mdi-database-plus'),
(6, 'Remove Redis Cache', 'Redis cache entry was removed', '#E53935', 'mdi-database-minus'),
(7, 'Add Redis Server', 'Redis server was added to configuration', '#1976D2', 'mdi-server-plus'),
(8, 'Remove Redis Server', 'Redis server was removed from configuration', '#D32F2F', 'mdi-server-minus'),
(9, 'Add RabbitMQ Server', 'RabbitMQ server was added to configuration', '#1976D2', 'mdi-server-plus'),
(10, 'Remove RabbitMQ Server', 'RabbitMQ server was removed from configuration', '#D32F2F', 'mdi-server-minus'),
(11, 'Add RabbitMQ Exchange', 'RabbitMQ exchange was created', '#4CAF50', 'mdi-swap-horizontal'),
(12, 'Remove RabbitMQ Exchange', 'RabbitMQ exchange was deleted', '#FF5722', 'mdi-swap-horizontal'),
(13, 'Add RabbitMQ Queue', 'RabbitMQ queue was created', '#4CAF50', 'mdi-message-processing'),
(14, 'Remove RabbitMQ Queue', 'RabbitMQ queue was deleted', '#FF5722', 'mdi-message-processing');
