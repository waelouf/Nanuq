import { describe, it, expect, beforeEach, vi } from 'vitest';
import { createStore } from 'vuex';
import sqliteModule from '../sqlite';
import apiClient from '@/services/apiClient';

vi.mock('@/services/apiClient');

describe('SQLite Store', () => {
  let store;

  beforeEach(() => {
    store = createStore({
      modules: {
        sqlite: sqliteModule,
      },
    });
    vi.clearAllMocks();
  });

  describe('State', () => {
    it('should initialize with empty arrays', () => {
      expect(store.state.sqlite.kafkaServers).toEqual([]);
      expect(store.state.sqlite.redisServers).toEqual([]);
      expect(store.state.sqlite.rabbitMQServers).toEqual([]);
    });
  });

  describe('Mutations', () => {
    it('should update kafka servers list', () => {
      const servers = [
        { id: 1, name: 'server1', url: 'localhost:9092' },
        { id: 2, name: 'server2', url: 'localhost:9093' },
      ];

      store.commit('sqlite/updateKafkaServers', servers);

      expect(store.state.sqlite.kafkaServers).toEqual(servers);
    });

    it('should update redis servers list', () => {
      const servers = [
        { id: 1, name: 'server1', url: 'localhost:6379' },
      ];

      store.commit('sqlite/updateRedisServers', servers);

      expect(store.state.sqlite.redisServers).toEqual(servers);
    });

    it('should update rabbitMQ servers list', () => {
      const servers = [
        { id: 1, name: 'server1', url: 'localhost:5672' },
      ];

      store.commit('sqlite/updateRabbitMQServers', servers);

      expect(store.state.sqlite.rabbitMQServers).toEqual(servers);
    });
  });

  describe('Actions', () => {
    it('should load kafka servers successfully', async () => {
      const mockServers = [
        { id: 1, name: 'server1', url: 'localhost:9092' },
      ];

      apiClient.get.mockResolvedValue({ data: mockServers });

      await store.dispatch('sqlite/loadKafkaServers');

      expect(apiClient.get).toHaveBeenCalledWith('/kafka/servers');
      expect(store.state.sqlite.kafkaServers).toEqual(mockServers);
    });

    it('should add kafka server successfully', async () => {
      const newServer = { name: 'new-server', url: 'localhost:9094' };

      apiClient.post.mockResolvedValue({ data: newServer });

      await store.dispatch('sqlite/addKafkaServer', newServer);

      expect(apiClient.post).toHaveBeenCalledWith('/kafka/servers', newServer);
    });

    it('should delete kafka server successfully', async () => {
      const serverId = 1;

      apiClient.delete.mockResolvedValue({ data: {} });

      await store.dispatch('sqlite/deleteKafkaServer', serverId);

      expect(apiClient.delete).toHaveBeenCalledWith(`/kafka/servers/${serverId}`);
    });
  });
});
