import { describe, it, expect, beforeEach, vi } from 'vitest';
import { createStore } from 'vuex';
import redisModule from '../redis';
import apiClient from '@/services/apiClient';

vi.mock('@/services/apiClient');

describe('Redis Store', () => {
  let store;

  beforeEach(() => {
    store = createStore({
      modules: {
        redis: redisModule,
      },
    });
    vi.clearAllMocks();
  });

  describe('State', () => {
    it('should initialize with empty state', () => {
      expect(store.state.redis.redisServers).toEqual({});
      expect(store.state.redis.redisDatabases).toEqual({});
      expect(store.state.redis.redisDatabaseKeys).toEqual({});
    });
  });

  describe('Mutations', () => {
    it('should update redis server info', () => {
      const serverUrl = 'localhost:6379';
      const serverInfo = { version: '7.0', uptime: 1000 };

      store.commit('redis/updateRedisServer', { serverUrl, data: serverInfo });

      expect(store.state.redis.redisServers[serverUrl]).toEqual(serverInfo);
    });

    it('should update redis databases', () => {
      const serverUrl = 'localhost:6379';
      const databases = [{ id: 0 }, { id: 1 }];

      store.commit('redis/updateRedisDatabases', { serverUrl, data: databases });

      expect(store.state.redis.redisDatabases[serverUrl]).toEqual(databases);
    });

    it('should update database keys', () => {
      const serverUrl = 'localhost:6379';
      const databaseId = '0';
      const keys = ['key1', 'key2', 'key3'];

      store.commit('redis/updateDatabaseKeys', { serverUrl, databaseId, data: keys });

      const key = `${serverUrl}-${databaseId}`;
      expect(store.state.redis.redisDatabaseKeys[key]).toEqual(keys);
    });
  });

  describe('Actions', () => {
    it('should load redis server info successfully', async () => {
      const serverUrl = 'localhost:6379';
      const mockServerInfo = { version: '7.0', uptime: 1000 };

      apiClient.get.mockResolvedValue({ data: mockServerInfo });

      await store.dispatch('redis/loadRedisServer', serverUrl);

      expect(apiClient.get).toHaveBeenCalledWith(`/redis/server/${serverUrl}`);
      expect(store.state.redis.redisServers[serverUrl]).toEqual(mockServerInfo);
    });

    it('should load redis databases successfully', async () => {
      const serverUrl = 'localhost:6379';
      const mockDatabases = [{ id: 0 }, { id: 1 }];

      apiClient.get.mockResolvedValue({ data: mockDatabases });

      await store.dispatch('redis/loadRedisDatabases', serverUrl);

      expect(apiClient.get).toHaveBeenCalledWith(`/redis/database/${serverUrl}`);
      expect(store.state.redis.redisDatabases[serverUrl]).toEqual(mockDatabases);
    });

    it('should invalidate cache successfully', async () => {
      const cacheKey = 'test-key';
      const serverUrl = 'localhost:6379';

      apiClient.delete.mockResolvedValue({ data: {} });

      await store.dispatch('redis/invalidateCache', { cacheKey, serverUrl });

      expect(apiClient.delete).toHaveBeenCalledWith(`redis/cache/${serverUrl}/${cacheKey}`);
    });
  });
});
