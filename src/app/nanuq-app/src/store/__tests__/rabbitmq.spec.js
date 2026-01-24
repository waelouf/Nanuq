import { describe, it, expect, beforeEach, vi } from 'vitest';
import { createStore } from 'vuex';
import rabbitmqModule from '../rabbitmq';
import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';
import { createMockError } from '@/__tests__/helpers/testHelpers';
import {
  mockRabbitMQExchanges,
  mockRabbitMQQueues,
  mockRabbitMQQueueDetails,
} from '@/__tests__/fixtures/mockData';

vi.mock('@/services/apiClient');
vi.mock('@/utils/logger');

describe('RabbitMQ Store', () => {
  let store;

  beforeEach(() => {
    store = createStore({
      modules: {
        rabbitmq: rabbitmqModule,
      },
    });
    vi.clearAllMocks();
  });

  // ==================== State Tests ====================
  describe('State', () => {
    it('should initialize with correct default state structure', () => {
      const state = store.state.rabbitmq;

      expect(state.exchanges).toEqual({});
      expect(state.queues).toEqual({});
      expect(state.queueDetails).toEqual({});
    });
  });

  // ==================== Getters Tests ====================
  describe('Getters', () => {
    it('should get exchanges for a server', () => {
      const serverUrl = 'localhost:5672';
      store.state.rabbitmq.exchanges[serverUrl] = mockRabbitMQExchanges;

      const exchanges = store.getters['rabbitmq/getExchanges'](serverUrl);
      expect(exchanges).toEqual(mockRabbitMQExchanges);
    });

    it('should return empty array when no exchanges exist for server', () => {
      const exchanges = store.getters['rabbitmq/getExchanges']('nonexistent-server');
      expect(exchanges).toEqual([]);
    });

    it('should get queues for a server', () => {
      const serverUrl = 'localhost:5672';
      store.state.rabbitmq.queues[serverUrl] = mockRabbitMQQueues;

      const queues = store.getters['rabbitmq/getQueues'](serverUrl);
      expect(queues).toEqual(mockRabbitMQQueues);
    });

    it('should return empty array when no queues exist for server', () => {
      const queues = store.getters['rabbitmq/getQueues']('nonexistent-server');
      expect(queues).toEqual([]);
    });

    it('should get queue details for a specific queue', () => {
      const serverUrl = 'localhost:5672';
      const queueName = 'test-queue-1';
      const key = `${serverUrl}-${queueName}`;
      store.state.rabbitmq.queueDetails[key] = mockRabbitMQQueueDetails;

      const details = store.getters['rabbitmq/getQueueDetails'](serverUrl, queueName);
      expect(details).toEqual(mockRabbitMQQueueDetails);
    });

    it('should return undefined when queue details do not exist', () => {
      const details = store.getters['rabbitmq/getQueueDetails']('nonexistent-server', 'nonexistent-queue');
      expect(details).toBeUndefined();
    });
  });

  // ==================== Mutations Tests ====================
  describe('Mutations', () => {
    it('should update exchanges for a server', () => {
      const serverUrl = 'localhost:5672';
      const data = mockRabbitMQExchanges;

      store.commit('rabbitmq/updateExchanges', { serverUrl, data });

      expect(store.state.rabbitmq.exchanges[serverUrl]).toEqual(data);
    });

    it('should replace existing exchanges when updating', () => {
      const serverUrl = 'localhost:5672';
      const oldData = [{ name: 'old-exchange' }];
      const newData = mockRabbitMQExchanges;

      store.state.rabbitmq.exchanges[serverUrl] = oldData;
      store.commit('rabbitmq/updateExchanges', { serverUrl, data: newData });

      expect(store.state.rabbitmq.exchanges[serverUrl]).toEqual(newData);
      expect(store.state.rabbitmq.exchanges[serverUrl]).not.toEqual(oldData);
    });

    it('should update queues for a server', () => {
      const serverUrl = 'localhost:5672';
      const data = mockRabbitMQQueues;

      store.commit('rabbitmq/updateQueues', { serverUrl, data });

      expect(store.state.rabbitmq.queues[serverUrl]).toEqual(data);
    });

    it('should replace existing queues when updating', () => {
      const serverUrl = 'localhost:5672';
      const oldData = [{ name: 'old-queue' }];
      const newData = mockRabbitMQQueues;

      store.state.rabbitmq.queues[serverUrl] = oldData;
      store.commit('rabbitmq/updateQueues', { serverUrl, data: newData });

      expect(store.state.rabbitmq.queues[serverUrl]).toEqual(newData);
      expect(store.state.rabbitmq.queues[serverUrl]).not.toEqual(oldData);
    });

    it('should update queue details', () => {
      const serverUrl = 'localhost:5672';
      const queueName = 'test-queue-1';
      const data = mockRabbitMQQueueDetails;

      store.commit('rabbitmq/updateQueueDetails', { serverUrl, queueName, data });

      const key = `${serverUrl}-${queueName}`;
      expect(store.state.rabbitmq.queueDetails[key]).toEqual(data);
    });

    it('should replace existing queue details when updating', () => {
      const serverUrl = 'localhost:5672';
      const queueName = 'test-queue-1';
      const key = `${serverUrl}-${queueName}`;
      const oldData = { messages: 5 };
      const newData = mockRabbitMQQueueDetails;

      store.state.rabbitmq.queueDetails[key] = oldData;
      store.commit('rabbitmq/updateQueueDetails', { serverUrl, queueName, data: newData });

      expect(store.state.rabbitmq.queueDetails[key]).toEqual(newData);
      expect(store.state.rabbitmq.queueDetails[key]).not.toEqual(oldData);
    });
  });

  // ==================== Actions Tests ====================
  describe('Actions', () => {
    describe('Exchange Actions', () => {
      it('should load exchanges successfully', async () => {
        const serverUrl = 'localhost:5672';
        apiClient.get.mockResolvedValue({ data: mockRabbitMQExchanges });

        await store.dispatch('rabbitmq/loadExchanges', serverUrl);

        expect(apiClient.get).toHaveBeenCalledWith(`/rabbitmq/exchanges/${serverUrl}`);
        // Wait for promise to resolve and mutation to be committed
        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(store.state.rabbitmq.exchanges[serverUrl]).toEqual(mockRabbitMQExchanges);
      });

      it('should handle error when loading exchanges', async () => {
        const serverUrl = 'localhost:5672';
        const error = createMockError('Connection refused');
        apiClient.get.mockRejectedValue(error);

        await store.dispatch('rabbitmq/loadExchanges', serverUrl);

        // Wait for promise to reject and error handler to be called
        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(logger.handleApiError).toHaveBeenCalledWith('RabbitMQStore', 'loading exchanges', error);
      });

      it('should add exchange successfully', async () => {
        const exchangeDetails = {
          serverUrl: 'localhost:5672',
          name: 'new-exchange',
          type: 'direct',
        };
        apiClient.post.mockResolvedValue({ data: {} });

        await store.dispatch('rabbitmq/addExchange', exchangeDetails);

        expect(apiClient.post).toHaveBeenCalledWith('/rabbitmq/exchange', exchangeDetails);
        // Wait for success handler
        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(logger.success).toHaveBeenCalledWith('Exchange created successfully');
      });

      it('should handle error when adding exchange', async () => {
        const exchangeDetails = {
          serverUrl: 'localhost:5672',
          name: 'new-exchange',
          type: 'direct',
        };
        const error = createMockError('Exchange already exists');
        apiClient.post.mockRejectedValue(error);

        await store.dispatch('rabbitmq/addExchange', exchangeDetails);

        // Wait for error handler
        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(logger.handleApiError).toHaveBeenCalledWith('RabbitMQStore', 'adding exchange', error);
      });

      it('should delete exchange successfully', async () => {
        const serverUrl = 'localhost:5672';
        const name = 'test-exchange';
        apiClient.delete.mockResolvedValue({ data: {} });

        await store.dispatch('rabbitmq/deleteExchange', { serverUrl, name });

        expect(apiClient.delete).toHaveBeenCalledWith(`/rabbitmq/exchange/${serverUrl}/${name}`);
        // Wait for success handler
        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(logger.success).toHaveBeenCalledWith('Exchange deleted successfully');
      });

      it('should handle error when deleting exchange', async () => {
        const serverUrl = 'localhost:5672';
        const name = 'test-exchange';
        const error = createMockError('Exchange not found', 404);
        apiClient.delete.mockRejectedValue(error);

        await store.dispatch('rabbitmq/deleteExchange', { serverUrl, name });

        // Wait for error handler
        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(logger.handleApiError).toHaveBeenCalledWith('RabbitMQStore', 'deleting exchange', error);
      });
    });

    describe('Queue Actions', () => {
      it('should load queues successfully', async () => {
        const serverUrl = 'localhost:5672';
        apiClient.get.mockResolvedValue({ data: mockRabbitMQQueues });

        await store.dispatch('rabbitmq/loadQueues', serverUrl);

        expect(apiClient.get).toHaveBeenCalledWith(`/rabbitmq/queues/${serverUrl}`);
        // Wait for promise to resolve
        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(store.state.rabbitmq.queues[serverUrl]).toEqual(mockRabbitMQQueues);
      });

      it('should handle error when loading queues', async () => {
        const serverUrl = 'localhost:5672';
        const error = createMockError('Connection refused');
        apiClient.get.mockRejectedValue(error);

        await store.dispatch('rabbitmq/loadQueues', serverUrl);

        // Wait for error handler
        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(logger.handleApiError).toHaveBeenCalledWith('RabbitMQStore', 'loading queues', error);
      });

      it('should load queue details successfully', async () => {
        const serverUrl = 'localhost:5672';
        const queueName = 'test-queue-1';
        apiClient.get.mockResolvedValue({ data: mockRabbitMQQueueDetails });

        await store.dispatch('rabbitmq/loadQueueDetails', { serverUrl, queueName });

        expect(apiClient.get).toHaveBeenCalledWith(`/rabbitmq/queue/${serverUrl}/${queueName}`);
        // Wait for promise to resolve
        await new Promise((resolve) => setTimeout(resolve, 0));
        const key = `${serverUrl}-${queueName}`;
        expect(store.state.rabbitmq.queueDetails[key]).toEqual(mockRabbitMQQueueDetails);
      });

      it('should handle error when loading queue details', async () => {
        const serverUrl = 'localhost:5672';
        const queueName = 'test-queue-1';
        const error = createMockError('Queue not found', 404);
        apiClient.get.mockRejectedValue(error);

        await store.dispatch('rabbitmq/loadQueueDetails', { serverUrl, queueName });

        // Wait for error handler
        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(logger.handleApiError).toHaveBeenCalledWith('RabbitMQStore', 'loading queue details', error);
      });

      it('should add queue successfully', async () => {
        const queueDetails = {
          serverUrl: 'localhost:5672',
          name: 'new-queue',
          durable: true,
        };
        apiClient.post.mockResolvedValue({ data: {} });

        await store.dispatch('rabbitmq/addQueue', queueDetails);

        expect(apiClient.post).toHaveBeenCalledWith('/rabbitmq/queue', queueDetails);
        // Wait for success handler
        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(logger.success).toHaveBeenCalledWith('Queue created successfully');
      });

      it('should handle error when adding queue', async () => {
        const queueDetails = {
          serverUrl: 'localhost:5672',
          name: 'new-queue',
          durable: true,
        };
        const error = createMockError('Queue already exists');
        apiClient.post.mockRejectedValue(error);

        await store.dispatch('rabbitmq/addQueue', queueDetails);

        // Wait for error handler
        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(logger.handleApiError).toHaveBeenCalledWith('RabbitMQStore', 'adding queue', error);
      });

      it('should delete queue successfully', async () => {
        const serverUrl = 'localhost:5672';
        const name = 'test-queue-1';
        apiClient.delete.mockResolvedValue({ data: {} });

        await store.dispatch('rabbitmq/deleteQueue', { serverUrl, name });

        expect(apiClient.delete).toHaveBeenCalledWith(`/rabbitmq/queue/${serverUrl}/${name}`);
        // Wait for success handler
        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(logger.success).toHaveBeenCalledWith('Queue deleted successfully');
      });

      it('should handle error when deleting queue', async () => {
        const serverUrl = 'localhost:5672';
        const name = 'test-queue-1';
        const error = createMockError('Queue not found', 404);
        apiClient.delete.mockRejectedValue(error);

        await store.dispatch('rabbitmq/deleteQueue', { serverUrl, name });

        // Wait for error handler
        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(logger.handleApiError).toHaveBeenCalledWith('RabbitMQStore', 'deleting queue', error);
      });
    });
  });
});
