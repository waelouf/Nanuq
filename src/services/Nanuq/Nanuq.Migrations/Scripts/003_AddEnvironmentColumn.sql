-- Add Environment column to server tables for multi-environment support
-- Allows users to organize servers by environment (Development, Staging, Production)

-- Add Environment column to Kafka table
ALTER TABLE Kafka ADD COLUMN Environment TEXT NOT NULL DEFAULT 'Development';

-- Add Environment column to Redis table
ALTER TABLE Redis ADD COLUMN Environment TEXT NOT NULL DEFAULT 'Development';

-- Add Environment column to RabbitMQ table
ALTER TABLE rabbit_mq ADD COLUMN Environment TEXT NOT NULL DEFAULT 'Development';
