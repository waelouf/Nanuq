import { describe, it, expect, beforeEach, vi } from 'vitest';
import { createStore } from 'vuex';
import kafkaModule from '../kafka';
import apiClient from '@/services/apiClient';

vi.mock('@/services/apiClient');

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
  });
});
