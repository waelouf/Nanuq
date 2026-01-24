import { describe, it, expect, beforeEach, vi } from 'vitest';
import { createStore } from 'vuex';
import kafkaModule from '../kafka';
import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';
import { createMockError } from '@/__tests__/helpers/testHelpers';

vi.mock('@/services/apiClient');
vi.mock('@/utils/logger');

describe('Kafka Store', () => {
  let store;

  beforeEach(() => {
    store = createStore({
      modules: {
        kafka: kafkaModule,
      },
    });
    vi.clearAllMocks();
  });

  describe('State', () => {
    it('should initialize with empty kafkaTopics', () => {
      expect(store.state.kafka.kafkaTopics).toEqual({});
    });

    it('should initialize with empty kafkaTopicDetails', () => {
      expect(store.state.kafka.kafkaTopicDetails).toEqual({});
    });
  });

  describe('Getters', () => {
    it('should return topic number of messages for valid key', () => {
      const key = 'test-server-test-topic';
      store.state.kafka.kafkaTopicDetails[key] = 150;

      const result = store.getters['kafka/getTopicNumberOfMessages'](key);

      expect(result).toBe(150);
    });

    it('should return undefined for non-existent key', () => {
      const result = store.getters['kafka/getTopicNumberOfMessages']('nonexistent-key');

      expect(result).toBeUndefined();
    });

    it('should return zero messages correctly', () => {
      const key = 'test-server-empty-topic';
      store.state.kafka.kafkaTopicDetails[key] = 0;

      const result = store.getters['kafka/getTopicNumberOfMessages'](key);

      expect(result).toBe(0);
    });
  });

  describe('Mutations', () => {
    it('should update topics for a server', () => {
      const serverName = 'test-server';
      const topics = [{ name: 'topic1' }, { name: 'topic2' }];

      store.commit('kafka/updateTopics', { serverName, data: topics });

      expect(store.state.kafka.kafkaTopics[serverName]).toEqual(topics);
    });

    it('should update topic details', () => {
      const serverName = 'test-server';
      const topicName = 'test-topic';
      const data = { numberOfMessages: 100 };

      store.commit('kafka/updateTopicDetails', { serverName, topicName, data });

      const key = `${serverName}-${topicName}`;
      expect(store.state.kafka.kafkaTopicDetails[key]).toBe(100);
    });

    it('should replace existing topics for a server', () => {
      const serverName = 'test-server';
      const oldTopics = [{ name: 'old-topic' }];
      const newTopics = [{ name: 'new-topic-1' }, { name: 'new-topic-2' }];

      store.commit('kafka/updateTopics', { serverName, data: oldTopics });
      expect(store.state.kafka.kafkaTopics[serverName]).toHaveLength(1);

      store.commit('kafka/updateTopics', { serverName, data: newTopics });
      expect(store.state.kafka.kafkaTopics[serverName]).toHaveLength(2);
      expect(store.state.kafka.kafkaTopics[serverName]).toEqual(newTopics);
    });

    it('should handle empty topics array', () => {
      const serverName = 'test-server';
      const topics = [];

      store.commit('kafka/updateTopics', { serverName, data: topics });

      expect(store.state.kafka.kafkaTopics[serverName]).toEqual([]);
    });

    it('should handle topic details with zero messages', () => {
      const serverName = 'test-server';
      const topicName = 'empty-topic';
      const data = { numberOfMessages: 0 };

      store.commit('kafka/updateTopicDetails', { serverName, topicName, data });

      const key = `${serverName}-${topicName}`;
      expect(store.state.kafka.kafkaTopicDetails[key]).toBe(0);
    });

    it('should handle multiple servers in state', () => {
      const server1 = 'server1';
      const server2 = 'server2';
      const topics1 = [{ name: 'topic1' }];
      const topics2 = [{ name: 'topic2' }];

      store.commit('kafka/updateTopics', { serverName: server1, data: topics1 });
      store.commit('kafka/updateTopics', { serverName: server2, data: topics2 });

      expect(store.state.kafka.kafkaTopics[server1]).toEqual(topics1);
      expect(store.state.kafka.kafkaTopics[server2]).toEqual(topics2);
    });
  });

  describe('Actions', () => {
    it('should load kafka topics successfully', async () => {
      const serverName = 'test-server';
      const mockTopics = [{ name: 'topic1' }, { name: 'topic2' }];

      apiClient.get.mockResolvedValue({ data: mockTopics });

      await store.dispatch('kafka/loadKafkaTopics', serverName);

      expect(apiClient.get).toHaveBeenCalledWith(`/kafka/topic/${serverName}`);
      expect(store.state.kafka.kafkaTopics[serverName]).toEqual(mockTopics);
    });

    it('should load kafka topic details successfully', async () => {
      const serverName = 'test-server';
      const topicName = 'test-topic';
      const mockDetails = { numberOfMessages: 150 };

      apiClient.get.mockResolvedValue({ data: mockDetails });

      await store.dispatch('kafka/loadKafkaTopicDetails', { serverName, topicName });

      expect(apiClient.get).toHaveBeenCalledWith(`/kafka/topic/${serverName}/${topicName}`);
      const key = `${serverName}-${topicName}`;
      expect(store.state.kafka.kafkaTopicDetails[key]).toBe(150);
    });

    it('should add kafka topic successfully', async () => {
      const topicDetails = { name: 'new-topic', server: 'test-server' };

      apiClient.post.mockResolvedValue({ data: {} });

      await store.dispatch('kafka/addKafkaTopic', topicDetails);

      expect(apiClient.post).toHaveBeenCalledWith('kafka/topic', topicDetails);
    });

    it('should delete kafka topic successfully', async () => {
      const bootstrapServer = 'test-server';
      const topicName = 'test-topic';

      apiClient.delete.mockResolvedValue({ data: {} });

      await store.dispatch('kafka/deleteKafkaTopic', { bootstrapServer, topicName });

      expect(apiClient.delete).toHaveBeenCalledWith(`kafka/topic/${bootstrapServer}/${topicName}`);
    });

    // ==================== Error Handling Tests ====================

    it('should handle error when loading kafka topics fails', async () => {
      const serverName = 'test-server';
      const error = createMockError('Network error');

      apiClient.get.mockRejectedValue(error);

      await expect(store.dispatch('kafka/loadKafkaTopics', serverName)).rejects.toThrow('Network error');
    });

    it('should handle error when loading topic details fails', async () => {
      const serverName = 'test-server';
      const topicName = 'test-topic';
      const error = createMockError('Topic not found');

      apiClient.get.mockRejectedValue(error);

      // loadKafkaTopicDetails doesn't have error handling, so it will throw
      await expect(
        store.dispatch('kafka/loadKafkaTopicDetails', { serverName, topicName })
      ).rejects.toThrow();
    });

    it('should handle error when adding kafka topic fails', async () => {
      const topicDetails = { name: 'new-topic', server: 'test-server' };
      const error = createMockError('Failed to create topic');

      apiClient.post.mockRejectedValue(error);

      await store.dispatch('kafka/addKafkaTopic', topicDetails);

      expect(logger.handleApiError).toHaveBeenCalledWith(
        'KafkaStore',
        'adding Kafka topic',
        error
      );
    });

    it('should handle error when deleting kafka topic fails', async () => {
      const bootstrapServer = 'test-server';
      const topicName = 'test-topic';
      const error = createMockError('Topic has active consumers');

      apiClient.delete.mockRejectedValue(error);

      await store.dispatch('kafka/deleteKafkaTopic', { bootstrapServer, topicName });

      expect(logger.handleApiError).toHaveBeenCalledWith(
        'KafkaStore',
        'deleting Kafka topic',
        error
      );
    });

    // ==================== Success Logger Tests ====================

    it('should log success message when adding topic succeeds', async () => {
      const topicDetails = { name: 'new-topic', server: 'test-server' };

      apiClient.post.mockResolvedValue({ data: {} });

      await store.dispatch('kafka/addKafkaTopic', topicDetails);

      expect(logger.success).toHaveBeenCalledWith('Kafka topic created successfully');
    });

    it('should log success message when deleting topic succeeds', async () => {
      const bootstrapServer = 'test-server';
      const topicName = 'test-topic';

      apiClient.delete.mockResolvedValue({ data: {} });

      await store.dispatch('kafka/deleteKafkaTopic', { bootstrapServer, topicName });

      expect(logger.success).toHaveBeenCalledWith('Kafka topic deleted successfully');
    });

    // ==================== Edge Case Tests ====================

    it('should handle empty topics array from server', async () => {
      const serverName = 'test-server';
      const emptyTopics = [];

      apiClient.get.mockResolvedValue({ data: emptyTopics });

      await store.dispatch('kafka/loadKafkaTopics', serverName);

      expect(store.state.kafka.kafkaTopics[serverName]).toEqual([]);
    });

    it('should handle server name with special characters', async () => {
      const serverName = 'server-with-special.chars:9092';
      const mockTopics = [{ name: 'topic1' }];

      apiClient.get.mockResolvedValue({ data: mockTopics });

      await store.dispatch('kafka/loadKafkaTopics', serverName);

      expect(apiClient.get).toHaveBeenCalledWith(`/kafka/topic/${serverName}`);
      expect(store.state.kafka.kafkaTopics[serverName]).toEqual(mockTopics);
    });

    it('should handle topic name with special characters', async () => {
      const serverName = 'test-server';
      const topicName = 'topic.with-special_chars';
      const mockDetails = { numberOfMessages: 50 };

      apiClient.get.mockResolvedValue({ data: mockDetails });

      await store.dispatch('kafka/loadKafkaTopicDetails', { serverName, topicName });

      expect(apiClient.get).toHaveBeenCalledWith(`/kafka/topic/${serverName}/${topicName}`);
      const key = `${serverName}-${topicName}`;
      expect(store.state.kafka.kafkaTopicDetails[key]).toBe(50);
    });
  });
});
