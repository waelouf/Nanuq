/**
 * Mock Data Fixtures for Nanuq Frontend Tests
 * Provides sample data for testing various platform integrations
 */

// ==================== Kafka Mock Data ====================

export const mockKafkaServer = {
  id: 1,
  name: 'Test Kafka Server',
  bootstrapServer: 'localhost:9092',
  environment: 'Development',
  createdAt: '2024-01-01T00:00:00Z',
};

export const mockKafkaTopics = [
  { name: 'test-topic-1', partitions: 3, replicationFactor: 1 },
  { name: 'test-topic-2', partitions: 6, replicationFactor: 2 },
];

export const mockKafkaTopicDetails = {
  name: 'test-topic-1',
  partitions: 3,
  replicationFactor: 1,
  numberOfMessages: 150,
};

// ==================== Redis Mock Data ====================

export const mockRedisServer = {
  id: 2,
  name: 'Test Redis Server',
  serverUrl: 'localhost:6379',
  environment: 'Development',
  createdAt: '2024-01-01T00:00:00Z',
};

export const mockRedisServerInfo = {
  version: '7.0.0',
  uptime: 3600,
  connectedClients: 5,
  usedMemory: '1024000',
};

export const mockRedisDatabaseInfo = {
  database: 0,
  messagesCount: 100,
};

export const mockRedisKeys = ['user:1', 'user:2', 'session:abc123'];

export const mockRedisStringValue = {
  key: 'user:1',
  value: '{"name":"John Doe","email":"john@example.com"}',
  ttl: -1,
};

export const mockRedisListValue = {
  key: 'queue:tasks',
  values: ['task1', 'task2', 'task3'],
  length: 3,
};

export const mockRedisHashValue = {
  key: 'user:profile:1',
  fields: {
    name: 'John Doe',
    email: 'john@example.com',
    age: '30',
  },
};

export const mockRedisSetValue = {
  key: 'tags',
  members: ['javascript', 'vue', 'redis'],
  count: 3,
};

export const mockRedisSortedSetValue = {
  key: 'leaderboard',
  members: [
    { value: 'player1', score: 100 },
    { value: 'player2', score: 95 },
    { value: 'player3', score: 90 },
  ],
  count: 3,
};

export const mockRedisStreamValue = {
  key: 'events',
  entries: [
    { id: '1-0', fields: { event: 'login', user: 'john' } },
    { id: '2-0', fields: { event: 'logout', user: 'john' } },
  ],
  length: 2,
};

// ==================== RabbitMQ Mock Data ====================

export const mockRabbitMQServer = {
  id: 3,
  name: 'Test RabbitMQ Server',
  host: 'localhost',
  port: 5672,
  environment: 'Development',
  createdAt: '2024-01-01T00:00:00Z',
};

export const mockRabbitMQExchanges = [
  { name: 'amq.direct', type: 'direct', durable: true },
  { name: 'amq.topic', type: 'topic', durable: true },
  { name: 'test-exchange', type: 'fanout', durable: false },
];

export const mockRabbitMQQueues = [
  { name: 'test-queue-1', messages: 10, consumers: 2 },
  { name: 'test-queue-2', messages: 5, consumers: 1 },
];

export const mockRabbitMQQueueDetails = {
  name: 'test-queue-1',
  messages: 10,
  consumers: 2,
  state: 'running',
  node: 'rabbit@localhost',
};

// ==================== AWS Mock Data ====================

export const mockAwsServer = {
  id: 4,
  name: 'Test AWS Account',
  region: 'us-east-1',
  environment: 'Development',
  createdAt: '2024-01-01T00:00:00Z',
};

export const mockSqsQueues = [
  {
    queueName: 'test-queue-1',
    queueUrl: 'https://sqs.us-east-1.amazonaws.com/123456789012/test-queue-1',
    attributes: {
      ApproximateNumberOfMessages: '5',
      QueueArn: 'arn:aws:sqs:us-east-1:123456789012:test-queue-1',
    },
  },
  {
    queueName: 'test-queue-2.fifo',
    queueUrl: 'https://sqs.us-east-1.amazonaws.com/123456789012/test-queue-2.fifo',
    attributes: {
      ApproximateNumberOfMessages: '10',
      QueueArn: 'arn:aws:sqs:us-east-1:123456789012:test-queue-2.fifo',
      FifoQueue: 'true',
    },
  },
];

export const mockSqsQueueDetails = {
  queueUrl: 'https://sqs.us-east-1.amazonaws.com/123456789012/test-queue-1',
  attributes: {
    QueueArn: 'arn:aws:sqs:us-east-1:123456789012:test-queue-1',
    ApproximateNumberOfMessages: '5',
    ApproximateNumberOfMessagesNotVisible: '2',
    CreatedTimestamp: '1640995200',
  },
};

export const mockSqsMessages = [
  {
    messageId: 'msg-1',
    receiptHandle: 'receipt-1',
    body: '{"event":"test","data":"value1"}',
    attributes: {
      SentTimestamp: '1640995200000',
    },
  },
  {
    messageId: 'msg-2',
    receiptHandle: 'receipt-2',
    body: '{"event":"test","data":"value2"}',
    attributes: {
      SentTimestamp: '1640995300000',
    },
  },
];

export const mockSnsTopics = [
  {
    topicArn: 'arn:aws:sns:us-east-1:123456789012:test-topic-1',
    subscriptionsConfirmed: 2,
    subscriptionsPending: 0,
  },
  {
    topicArn: 'arn:aws:sns:us-east-1:123456789012:test-topic-2',
    subscriptionsConfirmed: 1,
    subscriptionsPending: 1,
  },
];

export const mockSnsTopicDetails = {
  topicArn: 'arn:aws:sns:us-east-1:123456789012:test-topic-1',
  attributes: {
    DisplayName: 'Test Topic 1',
    SubscriptionsConfirmed: '2',
    SubscriptionsPending: '0',
  },
};

export const mockSnsSubscriptions = [
  {
    subscriptionArn: 'arn:aws:sns:us-east-1:123456789012:test-topic-1:sub-1',
    protocol: 'email',
    endpoint: 'test@example.com',
    owner: '123456789012',
  },
  {
    subscriptionArn: 'arn:aws:sns:us-east-1:123456789012:test-topic-1:sub-2',
    protocol: 'sqs',
    endpoint: 'arn:aws:sqs:us-east-1:123456789012:test-queue',
    owner: '123456789012',
  },
];

// ==================== Azure Mock Data ====================

export const mockAzureServer = {
  id: 5,
  name: 'Test Azure Service Bus',
  namespace: 'test-namespace',
  region: 'eastus',
  environment: 'Development',
  createdAt: '2024-01-01T00:00:00Z',
};

export const mockAzureQueues = [
  {
    name: 'test-queue-1',
    messageCount: 15,
    sizeInBytes: 1024,
    status: 'Active',
  },
  {
    name: 'test-queue-2',
    messageCount: 8,
    sizeInBytes: 512,
    status: 'Active',
  },
];

export const mockAzureTopics = [
  {
    name: 'test-topic-1',
    subscriptionCount: 3,
    sizeInBytes: 2048,
    status: 'Active',
  },
  {
    name: 'test-topic-2',
    subscriptionCount: 1,
    sizeInBytes: 1024,
    status: 'Active',
  },
];

export const mockAzureSubscriptions = [
  {
    name: 'subscription-1',
    messageCount: 5,
    status: 'Active',
  },
  {
    name: 'subscription-2',
    messageCount: 10,
    status: 'Active',
  },
];

// ==================== Credentials Mock Data ====================

export const mockCredentialMetadata = {
  id: 1,
  serverId: 4,
  serverType: 'AWS',
  username: 'AKIAIOSFODNN7EXAMPLE',
  hasCredentials: true,
  hasPassword: true,
  hasAdditionalConfig: true,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
};

export const mockCredentialsMap = {
  'AWS-4': {
    id: 1,
    serverId: 4,
    serverType: 'AWS',
    username: 'AKIAIOSFODNN7EXAMPLE',
    hasCredentials: true,
    hasPassword: true,
    hasAdditionalConfig: true,
  },
  'Azure-5': {
    id: 2,
    serverId: 5,
    serverType: 'Azure',
    username: 'connection-string',
    hasCredentials: true,
    hasPassword: true,
    hasAdditionalConfig: false,
  },
};

// ==================== Activity Log Mock Data ====================

export const mockActivityTypes = [
  { id: 1, name: 'KafkaTopicCreated', description: 'Kafka topic created' },
  { id: 2, name: 'KafkaTopicDeleted', description: 'Kafka topic deleted' },
  { id: 3, name: 'RedisKeyDeleted', description: 'Redis key deleted' },
  { id: 4, name: 'SQSQueueCreated', description: 'SQS queue created' },
  { id: 5, name: 'SNSTopicCreated', description: 'SNS topic created' },
];

export const mockActivityLogs = [
  {
    id: 1,
    activityTypeId: 1,
    serverId: 1,
    serverType: 'Kafka',
    details: 'Created topic: test-topic-1',
    timestamp: '2024-01-01T10:00:00Z',
  },
  {
    id: 2,
    activityTypeId: 4,
    serverId: 4,
    serverType: 'AWS',
    details: 'Created queue: test-queue-1',
    timestamp: '2024-01-01T11:00:00Z',
  },
  {
    id: 3,
    activityTypeId: 3,
    serverId: 2,
    serverType: 'Redis',
    details: 'Deleted key: user:123',
    timestamp: '2024-01-01T12:00:00Z',
  },
];

export const mockActivityLogsWithTypes = [
  {
    id: 1,
    activityTypeId: 1,
    activityTypeName: 'KafkaTopicCreated',
    activityTypeDescription: 'Kafka topic created',
    serverId: 1,
    serverType: 'Kafka',
    details: 'Created topic: test-topic-1',
    timestamp: '2024-01-01T10:00:00Z',
  },
  {
    id: 2,
    activityTypeId: 4,
    activityTypeName: 'SQSQueueCreated',
    activityTypeDescription: 'SQS queue created',
    serverId: 4,
    serverType: 'AWS',
    details: 'Created queue: test-queue-1',
    timestamp: '2024-01-01T11:00:00Z',
  },
];

// ==================== Notification Mock Data ====================

export const mockNotificationState = {
  show: false,
  message: '',
  color: 'success',
  timeout: 4000,
};

export const mockSuccessNotification = {
  show: true,
  message: 'Operation completed successfully',
  color: 'success',
  timeout: 4000,
};

export const mockErrorNotification = {
  show: true,
  message: 'An error occurred',
  color: 'error',
  timeout: 6000,
};

export const mockWarningNotification = {
  show: true,
  message: 'Warning: Please check your configuration',
  color: 'warning',
  timeout: 5000,
};
