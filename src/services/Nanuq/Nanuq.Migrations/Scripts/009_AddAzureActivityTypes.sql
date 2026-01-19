-- Add Azure Service Bus activity types
INSERT OR REPLACE INTO activity_type (Id, Name, Description, Color, Icon) VALUES
(32, 'Add Azure Server', 'Azure server was added to configuration', '#0078D4', 'mdi-microsoft-azure'),
(33, 'Remove Azure Server', 'Azure server was removed from configuration', '#D32F2F', 'mdi-microsoft-azure'),
(34, 'Add Azure Service Bus Queue', 'Azure Service Bus queue was created', '#4CAF50', 'mdi-message-plus'),
(35, 'Remove Azure Service Bus Queue', 'Azure Service Bus queue was deleted', '#FF5722', 'mdi-message-minus'),
(36, 'Send Azure Service Bus Message', 'Message was sent to Azure Service Bus queue', '#2196F3', 'mdi-send'),
(37, 'Add Azure Service Bus Topic', 'Azure Service Bus topic was created', '#4CAF50', 'mdi-bullhorn-outline'),
(38, 'Remove Azure Service Bus Topic', 'Azure Service Bus topic was deleted', '#FF5722', 'mdi-bullhorn-outline'),
(39, 'Publish Azure Service Bus Message', 'Message was published to Azure Service Bus topic', '#2196F3', 'mdi-publish'),
(40, 'Add Azure Subscription', 'Azure Service Bus subscription was created', '#4CAF50', 'mdi-rss'),
(41, 'Remove Azure Subscription', 'Azure Service Bus subscription was deleted', '#FF5722', 'mdi-rss');
