import { describe, it, expect, beforeEach, vi } from 'vitest';
import { createStore } from 'vuex';
import azureModule from '../azure';
import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';
import { createMockError } from '@/__tests__/helpers/testHelpers';
import {
  mockAzureQueues,
  mockAzureTopics,
  mockAzureSubscriptions,
} from '@/__tests__/fixtures/mockData';

vi.mock('@/services/apiClient');
vi.mock('@/utils/logger', () => ({
  default: {
    success: vi.fn(),
    showSuccess: vi.fn(), // Note: Azure store uses showSuccess but logger.js only has success()
    error: vi.fn(),
    warn: vi.fn(),
    info: vi.fn(),
    handleApiError: vi.fn(),
  },
}));

describe('Azure Store', () => {
  let store;

  beforeEach(() => {
    store = createStore({
      modules: {
        azure: azureModule,
      },
    });
    vi.clearAllMocks();
  });

  // ==================== State Tests ====================
  describe('State', () => {
    it('should initialize with correct default state structure', () => {
      const state = store.state.azure;

      expect(state.queues).toEqual([]);
      expect(state.topics).toEqual([]);
      expect(state.subscriptions).toEqual([]);
      expect(state.loading).toBe(false);
      expect(state.error).toBe(null);
    });
  });

  // ==================== Mutations Tests ====================
  describe('Mutations', () => {
    it('should set queues', () => {
      store.commit('azure/setQueues', mockAzureQueues);
      expect(store.state.azure.queues).toEqual(mockAzureQueues);
    });

    it('should set topics', () => {
      store.commit('azure/setTopics', mockAzureTopics);
      expect(store.state.azure.topics).toEqual(mockAzureTopics);
    });

    it('should set subscriptions', () => {
      store.commit('azure/setSubscriptions', mockAzureSubscriptions);
      expect(store.state.azure.subscriptions).toEqual(mockAzureSubscriptions);
    });

    it('should set loading state to true', () => {
      store.commit('azure/setLoading', true);
      expect(store.state.azure.loading).toBe(true);
    });

    it('should set loading state to false', () => {
      store.commit('azure/setLoading', true);
      store.commit('azure/setLoading', false);
      expect(store.state.azure.loading).toBe(false);
    });

    it('should set error message', () => {
      const errorMsg = 'Failed to load queues';
      store.commit('azure/setError', errorMsg);
      expect(store.state.azure.error).toBe(errorMsg);
    });

    it('should clear error', () => {
      store.commit('azure/setError', 'Some error');
      store.commit('azure/clearError');
      expect(store.state.azure.error).toBe(null);
    });
  });

  // ==================== Actions Tests ====================
  describe('Actions', () => {
    describe('Queue Actions', () => {
      it('should load queues successfully', async () => {
        const serverId = 5;
        apiClient.get.mockResolvedValue({ data: mockAzureQueues });

        await store.dispatch('azure/loadQueues', serverId);

        expect(apiClient.get).toHaveBeenCalledWith(`/azure/servicebus/queues/${serverId}`);
        expect(store.state.azure.queues).toEqual(mockAzureQueues);
        expect(store.state.azure.loading).toBe(false);
        expect(store.state.azure.error).toBe(null);
      });

      it('should set loading state during loadQueues', async () => {
        const serverId = 5;
        let loadingDuringCall = false;

        apiClient.get.mockImplementation(() => {
          loadingDuringCall = store.state.azure.loading;
          return Promise.resolve({ data: mockAzureQueues });
        });

        await store.dispatch('azure/loadQueues', serverId);

        expect(loadingDuringCall).toBe(true);
        expect(store.state.azure.loading).toBe(false);
      });

      it('should handle error when loading queues', async () => {
        const serverId = 5;
        const error = createMockError('Network error');
        apiClient.get.mockRejectedValue(error);

        await expect(store.dispatch('azure/loadQueues', serverId)).rejects.toThrow('Network error');
        expect(store.state.azure.error).toBe('Failed to load queues');
        expect(store.state.azure.loading).toBe(false);
        expect(logger.handleApiError).toHaveBeenCalledWith('AzureStore', 'loading queues', error);
      });

      it('should create queue successfully', async () => {
        const queueDetails = {
          serverId: 5,
          queueName: 'new-queue',
        };
        apiClient.post.mockResolvedValue({ data: {} });
        apiClient.get.mockResolvedValue({ data: [...mockAzureQueues] });

        await store.dispatch('azure/createQueue', queueDetails);

        expect(apiClient.post).toHaveBeenCalledWith('/azure/servicebus/queue', queueDetails);
        expect(apiClient.get).toHaveBeenCalledWith(`/azure/servicebus/queues/${queueDetails.serverId}`);
        expect(logger.showSuccess).toHaveBeenCalledWith('Queue created successfully');
      });

      it('should handle error when creating queue', async () => {
        const queueDetails = { serverId: 5, queueName: 'new-queue' };
        const error = createMockError('Queue already exists');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('azure/createQueue', queueDetails)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AzureStore', 'creating queue', error);
      });

      it('should delete queue successfully', async () => {
        const serverId = 5;
        const queueName = 'test-queue';
        apiClient.delete.mockResolvedValue({ data: {} });
        apiClient.get.mockResolvedValue({ data: [] });

        await store.dispatch('azure/deleteQueue', { serverId, queueName });

        expect(apiClient.delete).toHaveBeenCalledWith('/azure/servicebus/queue', {
          data: { serverId, queueName },
        });
        expect(apiClient.get).toHaveBeenCalledWith(`/azure/servicebus/queues/${serverId}`);
        expect(logger.showSuccess).toHaveBeenCalledWith('Queue deleted successfully');
      });

      it('should handle error when deleting queue', async () => {
        const serverId = 5;
        const queueName = 'test-queue';
        const error = createMockError('Queue not found', 404);
        apiClient.delete.mockRejectedValue(error);

        await expect(store.dispatch('azure/deleteQueue', { serverId, queueName })).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AzureStore', 'deleting queue', error);
      });

      it('should send message successfully', async () => {
        const messageDetails = {
          serverId: 5,
          queueName: 'test-queue',
          messageBody: 'Test message',
        };
        apiClient.post.mockResolvedValue({ data: {} });

        await store.dispatch('azure/sendMessage', messageDetails);

        expect(apiClient.post).toHaveBeenCalledWith('/azure/servicebus/queue/message', messageDetails);
        expect(logger.showSuccess).toHaveBeenCalledWith('Message sent successfully');
      });

      it('should handle error when sending message', async () => {
        const messageDetails = { serverId: 5, queueName: 'test-queue', messageBody: 'test' };
        const error = createMockError('Invalid message');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('azure/sendMessage', messageDetails)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AzureStore', 'sending message', error);
      });

      it('should receive messages successfully', async () => {
        const serverId = 5;
        const queueName = 'test-queue';
        const mockMessages = [
          { body: 'message 1', messageId: '1' },
          { body: 'message 2', messageId: '2' },
        ];
        apiClient.get.mockResolvedValue({ data: mockMessages });

        const result = await store.dispatch('azure/receiveMessages', { serverId, queueName });

        expect(apiClient.get).toHaveBeenCalledWith(`/azure/servicebus/queue/${serverId}/${queueName}/messages`);
        expect(result).toEqual(mockMessages);
      });

      it('should handle error when receiving messages', async () => {
        const serverId = 5;
        const queueName = 'test-queue';
        const error = createMockError('Queue not found');
        apiClient.get.mockRejectedValue(error);

        await expect(
          store.dispatch('azure/receiveMessages', { serverId, queueName })
        ).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AzureStore', 'receiving messages', error);
      });
    });

    describe('Topic Actions', () => {
      it('should load topics successfully', async () => {
        const serverId = 5;
        apiClient.get.mockResolvedValue({ data: mockAzureTopics });

        await store.dispatch('azure/loadTopics', serverId);

        expect(apiClient.get).toHaveBeenCalledWith(`/azure/servicebus/topics/${serverId}`);
        expect(store.state.azure.topics).toEqual(mockAzureTopics);
        expect(store.state.azure.loading).toBe(false);
        expect(store.state.azure.error).toBe(null);
      });

      it('should set loading state during loadTopics', async () => {
        const serverId = 5;
        let loadingDuringCall = false;

        apiClient.get.mockImplementation(() => {
          loadingDuringCall = store.state.azure.loading;
          return Promise.resolve({ data: mockAzureTopics });
        });

        await store.dispatch('azure/loadTopics', serverId);

        expect(loadingDuringCall).toBe(true);
        expect(store.state.azure.loading).toBe(false);
      });

      it('should handle error when loading topics', async () => {
        const serverId = 5;
        const error = createMockError('Network error');
        apiClient.get.mockRejectedValue(error);

        await expect(store.dispatch('azure/loadTopics', serverId)).rejects.toThrow('Network error');
        expect(store.state.azure.error).toBe('Failed to load topics');
        expect(store.state.azure.loading).toBe(false);
        expect(logger.handleApiError).toHaveBeenCalledWith('AzureStore', 'loading topics', error);
      });

      it('should create topic successfully', async () => {
        const topicDetails = {
          serverId: 5,
          topicName: 'new-topic',
        };
        apiClient.post.mockResolvedValue({ data: {} });
        apiClient.get.mockResolvedValue({ data: [...mockAzureTopics] });

        await store.dispatch('azure/createTopic', topicDetails);

        expect(apiClient.post).toHaveBeenCalledWith('/azure/servicebus/topic', topicDetails);
        expect(apiClient.get).toHaveBeenCalledWith(`/azure/servicebus/topics/${topicDetails.serverId}`);
        expect(logger.showSuccess).toHaveBeenCalledWith('Topic created successfully');
      });

      it('should handle error when creating topic', async () => {
        const topicDetails = { serverId: 5, topicName: 'new-topic' };
        const error = createMockError('Topic already exists');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('azure/createTopic', topicDetails)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AzureStore', 'creating topic', error);
      });

      it('should delete topic successfully', async () => {
        const serverId = 5;
        const topicName = 'test-topic';
        apiClient.delete.mockResolvedValue({ data: {} });
        apiClient.get.mockResolvedValue({ data: [] });

        await store.dispatch('azure/deleteTopic', { serverId, topicName });

        expect(apiClient.delete).toHaveBeenCalledWith('/azure/servicebus/topic', {
          data: { serverId, topicName },
        });
        expect(apiClient.get).toHaveBeenCalledWith(`/azure/servicebus/topics/${serverId}`);
        expect(logger.showSuccess).toHaveBeenCalledWith('Topic deleted successfully');
      });

      it('should handle error when deleting topic', async () => {
        const serverId = 5;
        const topicName = 'test-topic';
        const error = createMockError('Topic not found', 404);
        apiClient.delete.mockRejectedValue(error);

        await expect(store.dispatch('azure/deleteTopic', { serverId, topicName })).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AzureStore', 'deleting topic', error);
      });

      it('should publish message successfully', async () => {
        const messageDetails = {
          serverId: 5,
          topicName: 'test-topic',
          messageBody: 'Test message',
        };
        apiClient.post.mockResolvedValue({ data: {} });

        await store.dispatch('azure/publishMessage', messageDetails);

        expect(apiClient.post).toHaveBeenCalledWith('/azure/servicebus/topic/message', messageDetails);
        expect(logger.showSuccess).toHaveBeenCalledWith('Message published successfully');
      });

      it('should handle error when publishing message', async () => {
        const messageDetails = { serverId: 5, topicName: 'test-topic', messageBody: 'test' };
        const error = createMockError('Invalid message');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('azure/publishMessage', messageDetails)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AzureStore', 'publishing message', error);
      });
    });

    describe('Subscription Actions', () => {
      it('should load subscriptions successfully', async () => {
        const serverId = 5;
        const topicName = 'test-topic';
        apiClient.get.mockResolvedValue({ data: mockAzureSubscriptions });

        await store.dispatch('azure/loadSubscriptions', { serverId, topicName });

        expect(apiClient.get).toHaveBeenCalledWith(
          `/azure/servicebus/topic/${serverId}/${topicName}/subscriptions`
        );
        expect(store.state.azure.subscriptions).toEqual(mockAzureSubscriptions);
      });

      it('should handle error when loading subscriptions', async () => {
        const serverId = 5;
        const topicName = 'test-topic';
        const error = createMockError('Topic not found');
        apiClient.get.mockRejectedValue(error);

        await expect(
          store.dispatch('azure/loadSubscriptions', { serverId, topicName })
        ).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AzureStore', 'loading subscriptions', error);
      });

      it('should create subscription successfully', async () => {
        const subscriptionDetails = {
          serverId: 5,
          topicName: 'test-topic',
          subscriptionName: 'new-subscription',
        };
        apiClient.post.mockResolvedValue({ data: {} });
        apiClient.get.mockResolvedValue({ data: [...mockAzureSubscriptions] });

        await store.dispatch('azure/createSubscription', subscriptionDetails);

        expect(apiClient.post).toHaveBeenCalledWith('/azure/servicebus/subscription', subscriptionDetails);
        expect(apiClient.get).toHaveBeenCalledWith(
          `/azure/servicebus/topic/${subscriptionDetails.serverId}/${subscriptionDetails.topicName}/subscriptions`
        );
        expect(logger.showSuccess).toHaveBeenCalledWith('Subscription created successfully');
      });

      it('should handle error when creating subscription', async () => {
        const subscriptionDetails = {
          serverId: 5,
          topicName: 'test-topic',
          subscriptionName: 'new-subscription',
        };
        const error = createMockError('Subscription already exists');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('azure/createSubscription', subscriptionDetails)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AzureStore', 'creating subscription', error);
      });

      it('should delete subscription successfully', async () => {
        const serverId = 5;
        const topicName = 'test-topic';
        const subscriptionName = 'test-subscription';
        apiClient.delete.mockResolvedValue({ data: {} });
        apiClient.get.mockResolvedValue({ data: [] });

        await store.dispatch('azure/deleteSubscription', { serverId, topicName, subscriptionName });

        expect(apiClient.delete).toHaveBeenCalledWith('/azure/servicebus/subscription', {
          data: { serverId, topicName, subscriptionName },
        });
        expect(apiClient.get).toHaveBeenCalledWith(
          `/azure/servicebus/topic/${serverId}/${topicName}/subscriptions`
        );
        expect(logger.showSuccess).toHaveBeenCalledWith('Subscription deleted successfully');
      });

      it('should handle error when deleting subscription', async () => {
        const serverId = 5;
        const topicName = 'test-topic';
        const subscriptionName = 'test-subscription';
        const error = createMockError('Subscription not found', 404);
        apiClient.delete.mockRejectedValue(error);

        await expect(
          store.dispatch('azure/deleteSubscription', { serverId, topicName, subscriptionName })
        ).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('AzureStore', 'deleting subscription', error);
      });
    });
  });
});
