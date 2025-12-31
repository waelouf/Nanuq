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
      expect(store.state.redis.redisDatabaseMessageCounter).toEqual({});
      expect(store.state.redis.redisCachedStrings).toEqual({});
      expect(store.state.redis.redisDatabaseKeys).toEqual({});
    });
  });

  describe('Mutations', () => {
    it('should update redis server info', () => {
      const serverUrl = 'localhost:6379';
      const serverDetails = { version: '7.0', uptime: 1000 };

      store.commit('redis/updateServers', { serverUrl, serverDetails });

      expect(store.state.redis.redisServers[serverUrl]).toEqual(serverDetails);
    });

    it('should update redis databases', () => {
      const serverUrl = 'localhost:6379';
      const database = 0;
      const messagesCount = 100;

      store.commit('redis/updateDatabase', { serverUrl, database, messagesCount });

      const key = `${serverUrl}_${database}`;
      expect(store.state.redis.redisDatabaseMessageCounter[key]).toEqual(messagesCount);
    });

    it('should update database keys', () => {
      const serverUrl = 'localhost:6379';
      const database = 0;
      const keys = ['key1', 'key2', 'key3'];

      store.commit('redis/updateDatabaseKeys', { serverUrl, database, keys });

      const key = `${serverUrl}_${database}`;
      expect(store.state.redis.redisDatabaseKeys[key]).toEqual(keys);
    });
  });

  describe('Actions', () => {
    it('should load redis server info successfully', async () => {
      const serverUrl = 'localhost:6379';
      const mockServerInfo = { version: '7.0', uptime: 1000 };

      apiClient.get.mockResolvedValue({ data: mockServerInfo });

      await store.dispatch('redis/getServerDetails', serverUrl);

      expect(apiClient.get).toHaveBeenCalledWith(`/redis/${serverUrl}`);
      expect(store.state.redis.redisServers[serverUrl]).toEqual(mockServerInfo);
    });

    it('should load redis databases successfully', async () => {
      const serverUrl = 'localhost:6379';
      const database = 0;
      const mockDatabaseInfo = { messagesCount: 100 };

      apiClient.get.mockResolvedValue({ data: mockDatabaseInfo });

      await store.dispatch('redis/getDatabaseDetails', { serverUrl, database });

      expect(apiClient.get).toHaveBeenCalledWith(`/redis/${serverUrl}/${database}`);
      const key = `${serverUrl}_${database}`;
      expect(store.state.redis.redisDatabaseMessageCounter[key]).toEqual(100);
    });

    it('should invalidate cache successfully', async () => {
      const key = 'test-key';
      const serverUrl = 'localhost:6379';
      const database = 0;

      apiClient.delete.mockResolvedValue({ data: {} });

      await store.dispatch('redis/invalidateCachedString', { serverUrl, database, key });

      expect(apiClient.delete).toHaveBeenCalledWith(`/redis/string/${serverUrl}/${database}/${key}`);
    });
  });
});
