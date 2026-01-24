import { describe, it, expect, beforeEach, vi } from 'vitest';
import { createStore } from 'vuex';
import awsModule from '../aws';
import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';
import {
  createMockError,
  createAwsAuthError,
} from '@/__tests__/helpers/testHelpers';
import {
  mockSqsQueues,
  mockSqsQueueDetails,
  mockSqsMessages,
  mockSnsTopics,
  mockSnsTopicDetails,
  mockSnsSubscriptions,
} from '@/__tests__/fixtures/mockData';

vi.mock('@/services/apiClient');
vi.mock('@/utils/logger');

describe('AWS Store', () => {
  let store;

  beforeEach(() => {
    store = createStore({
      modules: {
        aws: awsModule,
      },
    });
    vi.clearAllMocks();
  });

  // ==================== State Tests ====================
  describe('State', () => {
    it('should initialize with correct default state structure', () => {
      const state = store.state.aws;

      expect(state.sqsQueues).toEqual({});
      expect(state.sqsQueueDetails).toEqual({});
      expect(state.sqsMessages).toEqual({});
      expect(state.snsTopics).toEqual({});
      expect(state.snsTopicDetails).toEqual({});
      expect(state.snsSubscriptions).toEqual({});
      expect(state.loading).toBe(false);
      expect(state.error).toBe(null);
    });
  });

  // ==================== Mutations Tests ====================
  describe('Mutations', () => {
    describe('SQS Mutations', () => {
      it('should update SQS queues for a server', () => {
        const serverId = 4;
        const data = mockSqsQueues;

        store.commit('aws/updateSqsQueues', { serverId, data });

        expect(store.state.aws.sqsQueues[serverId]).toEqual(data);
      });

      it('should update SQS queue details with URL encoding', () => {
        const serverId = 4;
        const queueUrl = 'https://sqs.us-east-1.amazonaws.com/123/test-queue';
        const data = mockSqsQueueDetails;

        store.commit('aws/updateSqsQueueDetails', { serverId, queueUrl, data });

        const key = `${serverId}-${encodeURIComponent(queueUrl)}`;
        expect(store.state.aws.sqsQueueDetails[key]).toEqual(data);
      });

      it('should handle special characters in queue URL encoding', () => {
        const serverId = 4;
        const queueUrl = 'https://sqs.us-east-1.amazonaws.com/123/queue?param=value&other=test';
        const data = mockSqsQueueDetails;

        store.commit('aws/updateSqsQueueDetails', { serverId, queueUrl, data });

        const key = `${serverId}-${encodeURIComponent(queueUrl)}`;
        expect(store.state.aws.sqsQueueDetails[key]).toEqual(data);
        expect(key).toContain('%3F'); // ? encoded
        expect(key).toContain('%3D'); // = encoded
        expect(key).toContain('%26'); // & encoded
      });

      it('should update SQS messages for a queue', () => {
        const serverId = 4;
        const queueUrl = 'https://sqs.us-east-1.amazonaws.com/123/test-queue';
        const data = mockSqsMessages;

        store.commit('aws/updateSqsMessages', { serverId, queueUrl, data });

        const key = `${serverId}-${encodeURIComponent(queueUrl)}`;
        expect(store.state.aws.sqsMessages[key]).toEqual(data);
      });
    });

    describe('SNS Mutations', () => {
      it('should update SNS topics for a server', () => {
        const serverId = 4;
        const data = mockSnsTopics;

        store.commit('aws/updateSnsTopics', { serverId, data });

        expect(store.state.aws.snsTopics[serverId]).toEqual(data);
      });

      it('should update SNS topic details with ARN encoding', () => {
        const serverId = 4;
        const topicArn = 'arn:aws:sns:us-east-1:123456789012:test-topic';
        const data = mockSnsTopicDetails;

        store.commit('aws/updateSnsTopicDetails', { serverId, topicArn, data });

        const key = `${serverId}-${encodeURIComponent(topicArn)}`;
        expect(store.state.aws.snsTopicDetails[key]).toEqual(data);
      });

      it('should handle special characters in topic ARN encoding', () => {
        const serverId = 4;
        const topicArn = 'arn:aws:sns:us-east-1:123456789012:topic-with-special-chars_@#';
        const data = mockSnsTopicDetails;

        store.commit('aws/updateSnsTopicDetails', { serverId, topicArn, data });

        const key = `${serverId}-${encodeURIComponent(topicArn)}`;
        expect(store.state.aws.snsTopicDetails[key]).toEqual(data);
        expect(key).toContain('%40'); // @ encoded
        expect(key).toContain('%23'); // # encoded
      });

      it('should update SNS subscriptions for a topic', () => {
        const serverId = 4;
        const topicArn = 'arn:aws:sns:us-east-1:123456789012:test-topic';
        const data = mockSnsSubscriptions;

        store.commit('aws/updateSnsSubscriptions', { serverId, topicArn, data });

        const key = `${serverId}-${encodeURIComponent(topicArn)}`;
        expect(store.state.aws.snsSubscriptions[key]).toEqual(data);
      });
    });

    describe('Loading and Error Mutations', () => {
      it('should set loading state to true', () => {
        store.commit('aws/setLoading', true);
        expect(store.state.aws.loading).toBe(true);
      });

      it('should set loading state to false', () => {
        store.commit('aws/setLoading', true);
        store.commit('aws/setLoading', false);
        expect(store.state.aws.loading).toBe(false);
      });

      it('should set error message', () => {
        const errorMsg = 'Failed to load SQS queues';
        store.commit('aws/setError', errorMsg);
        expect(store.state.aws.error).toBe(errorMsg);
      });

      it('should clear error', () => {
        store.commit('aws/setError', 'Some error');
        store.commit('aws/clearError');
        expect(store.state.aws.error).toBe(null);
      });
    });
  });

  // ==================== Getters Tests ====================
  describe('Getters', () => {
    describe('SQS Getters', () => {
      it('should get SQS queues for a server', () => {
        const serverId = 4;
        store.state.aws.sqsQueues[serverId] = mockSqsQueues;

        const queues = store.getters['aws/getSqsQueues'](serverId);
        expect(queues).toEqual(mockSqsQueues);
      });

      it('should return empty array when no queues exist for server', () => {
        const queues = store.getters['aws/getSqsQueues'](999);
        expect(queues).toEqual([]);
      });

      it('should get SQS queue details with URL encoding', () => {
        const serverId = 4;
        const queueUrl = 'https://sqs.us-east-1.amazonaws.com/123/test-queue';
        const key = `${serverId}-${encodeURIComponent(queueUrl)}`;
        store.state.aws.sqsQueueDetails[key] = mockSqsQueueDetails;

        const details = store.getters['aws/getSqsQueueDetails'](serverId, queueUrl);
        expect(details).toEqual(mockSqsQueueDetails);
      });

      it('should handle special characters in queue URL when getting details', () => {
        const serverId = 4;
        const queueUrl = 'https://sqs.us-east-1.amazonaws.com/123/queue?test=1';
        const key = `${serverId}-${encodeURIComponent(queueUrl)}`;
        store.state.aws.sqsQueueDetails[key] = mockSqsQueueDetails;

        const details = store.getters['aws/getSqsQueueDetails'](serverId, queueUrl);
        expect(details).toEqual(mockSqsQueueDetails);
      });

      it('should get SQS messages for a queue', () => {
        const serverId = 4;
        const queueUrl = 'https://sqs.us-east-1.amazonaws.com/123/test-queue';
        const key = `${serverId}-${encodeURIComponent(queueUrl)}`;
        store.state.aws.sqsMessages[key] = mockSqsMessages;

        const messages = store.getters['aws/getSqsMessages'](serverId, queueUrl);
        expect(messages).toEqual(mockSqsMessages);
      });

      it('should return empty array when no messages exist', () => {
        const messages = store.getters['aws/getSqsMessages'](4, 'nonexistent-url');
        expect(messages).toEqual([]);
      });
    });

    describe('SNS Getters', () => {
      it('should get SNS topics for a server', () => {
        const serverId = 4;
        store.state.aws.snsTopics[serverId] = mockSnsTopics;

        const topics = store.getters['aws/getSnsTopics'](serverId);
        expect(topics).toEqual(mockSnsTopics);
      });

      it('should return empty array when no topics exist for server', () => {
        const topics = store.getters['aws/getSnsTopics'](999);
        expect(topics).toEqual([]);
      });

      it('should get SNS topic details with ARN encoding', () => {
        const serverId = 4;
        const topicArn = 'arn:aws:sns:us-east-1:123456789012:test-topic';
        const key = `${serverId}-${encodeURIComponent(topicArn)}`;
        store.state.aws.snsTopicDetails[key] = mockSnsTopicDetails;

        const details = store.getters['aws/getSnsTopicDetails'](serverId, topicArn);
        expect(details).toEqual(mockSnsTopicDetails);
      });

      it('should get SNS subscriptions for a topic', () => {
        const serverId = 4;
        const topicArn = 'arn:aws:sns:us-east-1:123456789012:test-topic';
        const key = `${serverId}-${encodeURIComponent(topicArn)}`;
        store.state.aws.snsSubscriptions[key] = mockSnsSubscriptions;

        const subscriptions = store.getters['aws/getSnsSubscriptions'](serverId, topicArn);
        expect(subscriptions).toEqual(mockSnsSubscriptions);
      });

      it('should return empty array when no subscriptions exist', () => {
        const subscriptions = store.getters['aws/getSnsSubscriptions'](4, 'nonexistent-arn');
        expect(subscriptions).toEqual([]);
      });
    });
  });

  // ==================== Actions Tests ====================
  describe('Actions', () => {
    describe('SQS Actions', () => {
      it('should load SQS queues successfully', async () => {
        const serverId = 4;
        apiClient.get.mockResolvedValue({ data: mockSqsQueues });

        await store.dispatch('aws/loadSqsQueues', serverId);

        expect(apiClient.get).toHaveBeenCalledWith(`/aws/sqs/queues/${serverId}`);
        expect(store.state.aws.sqsQueues[serverId]).toEqual(mockSqsQueues);
        expect(store.state.aws.loading).toBe(false);
        expect(store.state.aws.error).toBe(null);
      });

      it('should set loading state during loadSqsQueues', async () => {
        const serverId = 4;
        let loadingDuringCall = false;

        apiClient.get.mockImplementation(() => {
          loadingDuringCall = store.state.aws.loading;
          return Promise.resolve({ data: mockSqsQueues });
        });

        await store.dispatch('aws/loadSqsQueues', serverId);

        expect(loadingDuringCall).toBe(true);
        expect(store.state.aws.loading).toBe(false);
      });

      it('should handle AWS auth error in loadSqsQueues', async () => {
        const serverId = 4;
        const authError = createAwsAuthError('token');
        apiClient.get.mockRejectedValue(authError);

        await expect(store.dispatch('aws/loadSqsQueues', serverId)).rejects.toThrow('AWS_AUTH_ERROR');
        expect(store.state.aws.error).toBe('Failed to load SQS queues');
        expect(logger.handleApiError).toHaveBeenCalledWith('AWSStore', 'loading SQS queues', authError);
      });

      it('should handle generic error in loadSqsQueues', async () => {
        const serverId = 4;
        const error = createMockError('Network error');
        apiClient.get.mockRejectedValue(error);

        await expect(store.dispatch('aws/loadSqsQueues', serverId)).rejects.toThrow('Network error');
        expect(store.state.aws.error).toBe('Failed to load SQS queues');
        expect(store.state.aws.loading).toBe(false);
      });

      it('should load SQS queue details successfully', async () => {
        const serverId = 4;
        const queueUrl = 'https://sqs.us-east-1.amazonaws.com/123/test-queue';
        apiClient.get.mockResolvedValue({ data: mockSqsQueueDetails });

        await store.dispatch('aws/loadSqsQueueDetails', { serverId, queueUrl });

        expect(apiClient.get).toHaveBeenCalledWith(
          `/aws/sqs/queue/details/${serverId}?queueUrl=${encodeURIComponent(queueUrl)}`
        );
        const key = `${serverId}-${encodeURIComponent(queueUrl)}`;
        expect(store.state.aws.sqsQueueDetails[key]).toEqual(mockSqsQueueDetails);
      });

      it('should handle AWS auth error in loadSqsQueueDetails', async () => {
        const serverId = 4;
        const queueUrl = 'https://sqs.us-east-1.amazonaws.com/123/test-queue';
        const authError = createAwsAuthError('credentials');
        apiClient.get.mockRejectedValue(authError);

        await expect(
          store.dispatch('aws/loadSqsQueueDetails', { serverId, queueUrl })
        ).rejects.toThrow('AWS_AUTH_ERROR');
      });

      it('should create queue successfully', async () => {
        const queueDetails = {
          serverId: 4,
          queueName: 'new-queue',
          attributes: {},
        };
        apiClient.post.mockResolvedValue({ data: {} });
        apiClient.get.mockResolvedValue({ data: [...mockSqsQueues] });

        await store.dispatch('aws/createQueue', queueDetails);

        expect(apiClient.post).toHaveBeenCalledWith('/aws/sqs/queue/create', queueDetails);
        expect(apiClient.get).toHaveBeenCalledWith(`/aws/sqs/queues/${queueDetails.serverId}`);
        expect(logger.success).toHaveBeenCalledWith('SQS queue created successfully');
      });

      it('should handle error when creating queue', async () => {
        const queueDetails = { serverId: 4, queueName: 'new-queue' };
        const error = createMockError('Queue already exists');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('aws/createQueue', queueDetails)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AWSStore', 'creating queue', error);
      });

      it('should delete queue successfully', async () => {
        const serverId = 4;
        const queueUrl = 'https://sqs.us-east-1.amazonaws.com/123/test-queue';
        apiClient.delete.mockResolvedValue({ data: {} });
        apiClient.get.mockResolvedValue({ data: [] });

        await store.dispatch('aws/deleteQueue', { serverId, queueUrl });

        expect(apiClient.delete).toHaveBeenCalledWith(
          `/aws/sqs/queue/${serverId}?queueUrl=${encodeURIComponent(queueUrl)}`
        );
        expect(apiClient.get).toHaveBeenCalledWith(`/aws/sqs/queues/${serverId}`);
        expect(logger.success).toHaveBeenCalledWith('SQS queue deleted successfully');
      });

      it('should handle error when deleting queue', async () => {
        const serverId = 4;
        const queueUrl = 'https://sqs.us-east-1.amazonaws.com/123/test-queue';
        const error = createMockError('Queue not found', 404);
        apiClient.delete.mockRejectedValue(error);

        await expect(store.dispatch('aws/deleteQueue', { serverId, queueUrl })).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AWSStore', 'deleting queue', error);
      });

      it('should send message successfully', async () => {
        const messageDetails = {
          serverId: 4,
          queueUrl: 'https://sqs.us-east-1.amazonaws.com/123/test-queue',
          messageBody: 'Test message',
        };
        apiClient.post.mockResolvedValue({ data: { messageId: 'msg-123' } });

        await store.dispatch('aws/sendMessage', messageDetails);

        expect(apiClient.post).toHaveBeenCalledWith('/aws/sqs/message/send', messageDetails);
        expect(logger.success).toHaveBeenCalledWith('Message sent successfully');
      });

      it('should handle error when sending message', async () => {
        const messageDetails = { serverId: 4, queueUrl: 'test-url', messageBody: 'test' };
        const error = createMockError('Invalid message');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('aws/sendMessage', messageDetails)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AWSStore', 'sending message', error);
      });

      it('should receive messages successfully', async () => {
        const serverId = 4;
        const queueUrl = 'https://sqs.us-east-1.amazonaws.com/123/test-queue';
        const maxMessages = 10;
        apiClient.post.mockResolvedValue({ data: mockSqsMessages });

        const result = await store.dispatch('aws/receiveMessages', {
          serverId,
          queueUrl,
          maxMessages,
        });

        expect(apiClient.post).toHaveBeenCalledWith('/aws/sqs/message/receive', {
          serverId,
          queueUrl,
          maxMessages,
        });
        expect(result).toEqual(mockSqsMessages);
        const key = `${serverId}-${encodeURIComponent(queueUrl)}`;
        expect(store.state.aws.sqsMessages[key]).toEqual(mockSqsMessages);
      });

      it('should handle error when receiving messages', async () => {
        const params = { serverId: 4, queueUrl: 'test-url' };
        const error = createMockError('Queue not found');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('aws/receiveMessages', params)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AWSStore', 'receiving messages', error);
      });

      it('should delete message successfully', async () => {
        const serverId = 4;
        const queueUrl = 'https://sqs.us-east-1.amazonaws.com/123/test-queue';
        const receiptHandle = 'receipt-123';
        apiClient.delete.mockResolvedValue({ data: {} });

        await store.dispatch('aws/deleteMessage', { serverId, queueUrl, receiptHandle });

        expect(apiClient.delete).toHaveBeenCalledWith(
          `/aws/sqs/message/${serverId}?queueUrl=${encodeURIComponent(queueUrl)}&receiptHandle=${encodeURIComponent(receiptHandle)}`
        );
        expect(logger.success).toHaveBeenCalledWith('Message deleted successfully');
      });

      it('should handle error when deleting message', async () => {
        const params = { serverId: 4, queueUrl: 'test-url', receiptHandle: 'receipt' };
        const error = createMockError('Invalid receipt handle');
        apiClient.delete.mockRejectedValue(error);

        await expect(store.dispatch('aws/deleteMessage', params)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AWSStore', 'deleting message', error);
      });
    });

    describe('SNS Actions', () => {
      it('should load SNS topics successfully', async () => {
        const serverId = 4;
        apiClient.get.mockResolvedValue({ data: mockSnsTopics });

        await store.dispatch('aws/loadSnsTopics', serverId);

        expect(apiClient.get).toHaveBeenCalledWith(`/aws/sns/topics/${serverId}`);
        expect(store.state.aws.snsTopics[serverId]).toEqual(mockSnsTopics);
        expect(store.state.aws.loading).toBe(false);
        expect(store.state.aws.error).toBe(null);
      });

      it('should handle AWS auth error in loadSnsTopics', async () => {
        const serverId = 4;
        const authError = createAwsAuthError('unauthorized');
        apiClient.get.mockRejectedValue(authError);

        await expect(store.dispatch('aws/loadSnsTopics', serverId)).rejects.toThrow('AWS_AUTH_ERROR');
        expect(store.state.aws.error).toBe('Failed to load SNS topics');
      });

      it('should handle generic error in loadSnsTopics', async () => {
        const serverId = 4;
        const error = createMockError('Network error');
        apiClient.get.mockRejectedValue(error);

        await expect(store.dispatch('aws/loadSnsTopics', serverId)).rejects.toThrow('Network error');
        expect(store.state.aws.loading).toBe(false);
      });

      it('should load SNS topic details successfully', async () => {
        const serverId = 4;
        const topicArn = 'arn:aws:sns:us-east-1:123456789012:test-topic';
        apiClient.get.mockResolvedValue({ data: mockSnsTopicDetails });

        await store.dispatch('aws/loadSnsTopicDetails', { serverId, topicArn });

        expect(apiClient.get).toHaveBeenCalledWith(
          `/aws/sns/topic/details/${serverId}?topicArn=${encodeURIComponent(topicArn)}`
        );
        const key = `${serverId}-${encodeURIComponent(topicArn)}`;
        expect(store.state.aws.snsTopicDetails[key]).toEqual(mockSnsTopicDetails);
      });

      it('should handle AWS auth error in loadSnsTopicDetails', async () => {
        const serverId = 4;
        const topicArn = 'arn:aws:sns:us-east-1:123456789012:test-topic';
        const authError = createAwsAuthError('token');
        apiClient.get.mockRejectedValue(authError);

        await expect(
          store.dispatch('aws/loadSnsTopicDetails', { serverId, topicArn })
        ).rejects.toThrow('AWS_AUTH_ERROR');
      });

      it('should create SNS topic successfully', async () => {
        const topicDetails = {
          serverId: 4,
          topicName: 'new-topic',
        };
        apiClient.post.mockResolvedValue({ data: {} });
        apiClient.get.mockResolvedValue({ data: [...mockSnsTopics] });

        await store.dispatch('aws/createTopic', topicDetails);

        expect(apiClient.post).toHaveBeenCalledWith('/aws/sns/topic/create', topicDetails);
        expect(apiClient.get).toHaveBeenCalledWith(`/aws/sns/topics/${topicDetails.serverId}`);
        expect(logger.success).toHaveBeenCalledWith('SNS topic created successfully');
      });

      it('should handle error when creating topic', async () => {
        const topicDetails = { serverId: 4, topicName: 'new-topic' };
        const error = createMockError('Topic already exists');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('aws/createTopic', topicDetails)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AWSStore', 'creating topic', error);
      });

      it('should delete SNS topic successfully', async () => {
        const serverId = 4;
        const topicArn = 'arn:aws:sns:us-east-1:123456789012:test-topic';
        apiClient.delete.mockResolvedValue({ data: {} });
        apiClient.get.mockResolvedValue({ data: [] });

        await store.dispatch('aws/deleteTopic', { serverId, topicArn });

        expect(apiClient.delete).toHaveBeenCalledWith(
          `/aws/sns/topic/${serverId}?topicArn=${encodeURIComponent(topicArn)}`
        );
        expect(apiClient.get).toHaveBeenCalledWith(`/aws/sns/topics/${serverId}`);
        expect(logger.success).toHaveBeenCalledWith('SNS topic deleted successfully');
      });

      it('should handle error when deleting topic', async () => {
        const serverId = 4;
        const topicArn = 'arn:aws:sns:us-east-1:123456789012:test-topic';
        const error = createMockError('Topic not found', 404);
        apiClient.delete.mockRejectedValue(error);

        await expect(store.dispatch('aws/deleteTopic', { serverId, topicArn })).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AWSStore', 'deleting topic', error);
      });

      it('should publish message successfully', async () => {
        const messageDetails = {
          serverId: 4,
          topicArn: 'arn:aws:sns:us-east-1:123456789012:test-topic',
          message: 'Test message',
        };
        apiClient.post.mockResolvedValue({ data: { messageId: 'msg-123' } });

        await store.dispatch('aws/publishMessage', messageDetails);

        expect(apiClient.post).toHaveBeenCalledWith('/aws/sns/message/publish', messageDetails);
        expect(logger.success).toHaveBeenCalledWith('Message published successfully');
      });

      it('should handle error when publishing message', async () => {
        const messageDetails = { serverId: 4, topicArn: 'test-arn', message: 'test' };
        const error = createMockError('Invalid message');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('aws/publishMessage', messageDetails)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AWSStore', 'publishing message', error);
      });

      it('should load subscriptions successfully', async () => {
        const serverId = 4;
        const topicArn = 'arn:aws:sns:us-east-1:123456789012:test-topic';
        apiClient.get.mockResolvedValue({ data: mockSnsSubscriptions });

        await store.dispatch('aws/loadSubscriptions', { serverId, topicArn });

        expect(apiClient.get).toHaveBeenCalledWith(
          `/aws/sns/subscriptions/${serverId}?topicArn=${encodeURIComponent(topicArn)}`
        );
        const key = `${serverId}-${encodeURIComponent(topicArn)}`;
        expect(store.state.aws.snsSubscriptions[key]).toEqual(mockSnsSubscriptions);
      });

      it('should handle AWS auth error in loadSubscriptions', async () => {
        const serverId = 4;
        const topicArn = 'arn:aws:sns:us-east-1:123456789012:test-topic';
        const authError = createAwsAuthError('token');
        apiClient.get.mockRejectedValue(authError);

        await expect(
          store.dispatch('aws/loadSubscriptions', { serverId, topicArn })
        ).rejects.toThrow('AWS_AUTH_ERROR');
      });

      it('should subscribe successfully', async () => {
        const subscriptionDetails = {
          serverId: 4,
          topicArn: 'arn:aws:sns:us-east-1:123456789012:test-topic',
          protocol: 'email',
          endpoint: 'test@example.com',
        };
        apiClient.post.mockResolvedValue({ data: {} });
        apiClient.get.mockResolvedValue({ data: [...mockSnsSubscriptions] });

        await store.dispatch('aws/subscribe', subscriptionDetails);

        expect(apiClient.post).toHaveBeenCalledWith('/aws/sns/subscription/subscribe', subscriptionDetails);
        expect(apiClient.get).toHaveBeenCalledWith(
          `/aws/sns/subscriptions/${subscriptionDetails.serverId}?topicArn=${encodeURIComponent(subscriptionDetails.topicArn)}`
        );
        expect(logger.success).toHaveBeenCalledWith('Subscribed successfully');
      });

      it('should handle error when subscribing', async () => {
        const subscriptionDetails = {
          serverId: 4,
          topicArn: 'test-arn',
          protocol: 'email',
          endpoint: 'test@example.com',
        };
        const error = createMockError('Invalid endpoint');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('aws/subscribe', subscriptionDetails)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AWSStore', 'subscribing', error);
      });

      it('should unsubscribe successfully', async () => {
        const serverId = 4;
        const topicArn = 'arn:aws:sns:us-east-1:123456789012:test-topic';
        const subscriptionArn = 'arn:aws:sns:us-east-1:123456789012:test-topic:sub-123';
        apiClient.delete.mockResolvedValue({ data: {} });
        apiClient.get.mockResolvedValue({ data: [] });

        await store.dispatch('aws/unsubscribe', { serverId, topicArn, subscriptionArn });

        expect(apiClient.delete).toHaveBeenCalledWith(
          `/aws/sns/subscription/${serverId}?subscriptionArn=${encodeURIComponent(subscriptionArn)}`
        );
        expect(apiClient.get).toHaveBeenCalledWith(
          `/aws/sns/subscriptions/${serverId}?topicArn=${encodeURIComponent(topicArn)}`
        );
        expect(logger.success).toHaveBeenCalledWith('Unsubscribed successfully');
      });

      it('should handle error when unsubscribing', async () => {
        const params = {
          serverId: 4,
          topicArn: 'test-arn',
          subscriptionArn: 'sub-arn',
        };
        const error = createMockError('Subscription not found');
        apiClient.delete.mockRejectedValue(error);

        await expect(store.dispatch('aws/unsubscribe', params)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AWSStore', 'unsubscribing', error);
      });
    });
  });
});
