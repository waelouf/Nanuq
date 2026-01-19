-- Migration 008: Add AWS Activity Types
-- Adds activity types 23-31 for AWS SNS/SQS operations

INSERT OR REPLACE INTO activity_type (Id, Name, Description, Color, Icon) VALUES
(23, 'Add AWS Server', 'AWS server was added to configuration', '#FF9900', 'mdi-aws'),
(24, 'Remove AWS Server', 'AWS server was removed from configuration', '#D32F2F', 'mdi-aws'),
(25, 'Add SQS Queue', 'SQS queue was created', '#4CAF50', 'mdi-message-processing'),
(26, 'Remove SQS Queue', 'SQS queue was deleted', '#FF5722', 'mdi-message-processing'),
(27, 'Send SQS Message', 'Message sent to SQS queue', '#2196F3', 'mdi-message-arrow-right'),
(28, 'Add SNS Topic', 'SNS topic was created', '#4CAF50', 'mdi-bullhorn'),
(29, 'Remove SNS Topic', 'SNS topic was deleted', '#FF5722', 'mdi-bullhorn'),
(30, 'Publish SNS Message', 'Message published to SNS topic', '#2196F3', 'mdi-bullhorn-outline'),
(31, 'Add SNS Subscription', 'Subscription created for SNS topic', '#4CAF50', 'mdi-bell-plus');
