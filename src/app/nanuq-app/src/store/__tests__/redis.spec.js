import { describe, it, expect, beforeEach, vi } from 'vitest';
import { createStore } from 'vuex';
import redisModule from '../redis';
import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';
import { createMockError } from '@/__tests__/helpers/testHelpers';
import {
  mockRedisServerInfo,
  mockRedisDatabaseInfo,
  mockRedisKeys,
  mockRedisStringValue,
  mockRedisListValue,
  mockRedisHashValue,
  mockRedisSetValue,
  mockRedisSortedSetValue,
  mockRedisStreamValue,
} from '@/__tests__/fixtures/mockData';

vi.mock('@/services/apiClient');
vi.mock('@/utils/logger');

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

    // String Operations
    describe('String Operations', () => {
      const serverUrl = 'localhost:6379';
      const database = 0;

      it('should cache string successfully', async () => {
        const cacheObject = { serverUrl, database, key: 'user:1', value: 'John Doe' };
        apiClient.post.mockResolvedValue({ data: {} });

        await store.dispatch('redis/cacheString', cacheObject);

        expect(apiClient.post).toHaveBeenCalledWith('/redis/string', cacheObject);
        expect(logger.success).toHaveBeenCalledWith('String cached successfully');
      });

      it('should handle error when caching string', async () => {
        const cacheObject = { serverUrl, database, key: 'user:1', value: 'John Doe' };
        const error = createMockError('Network error');
        apiClient.post.mockRejectedValue(error);

        await store.dispatch('redis/cacheString', cacheObject);

        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(logger.handleApiError).toHaveBeenCalledWith('RedisStore', 'caching string', error);
      });

      it('should get cached string successfully', async () => {
        const key = 'user:1';
        apiClient.get.mockResolvedValue({ data: mockRedisStringValue });

        store.dispatch('redis/getCachedString', { serverUrl, database, key });

        await new Promise((resolve) => setTimeout(resolve, 0));
        expect(apiClient.get).toHaveBeenCalledWith(`/redis/string/${serverUrl}/${database}/${key}`);
        const storeKey = `${serverUrl}_${database}_${key}`;
        expect(store.state.redis.redisCachedStrings[storeKey]).toEqual(mockRedisStringValue);
      });

      it('should get all string keys successfully', async () => {
        apiClient.get.mockResolvedValue({ data: mockRedisKeys });

        await store.dispatch('redis/getAllStringKeys', { serverUrl, database });

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/string/${serverUrl}/${database}`);
        const storeKey = `${serverUrl}_${database}`;
        expect(store.state.redis.redisDatabaseKeys[storeKey]).toEqual(mockRedisKeys);
      });
    });

    // List Operations
    describe('List Operations', () => {
      const serverUrl = 'localhost:6379';
      const database = 0;
      const key = 'queue:tasks';

      it('should push element to list successfully', async () => {
        const params = { serverUrl, database, key, value: 'task1', pushLeft: false };
        apiClient.post.mockResolvedValue({ data: 5 });

        const result = await store.dispatch('redis/pushListElement', params);

        expect(apiClient.post).toHaveBeenCalledWith('/redis/list/push', params);
        expect(logger.success).toHaveBeenCalledWith('Element pushed to list successfully (length: 5)');
        expect(result).toBe(5);
      });

      it('should push element to left of list', async () => {
        const params = { serverUrl, database, key, value: 'task1', pushLeft: true };
        apiClient.post.mockResolvedValue({ data: 3 });

        await store.dispatch('redis/pushListElement', params);

        expect(apiClient.post).toHaveBeenCalledWith('/redis/list/push', params);
      });

      it('should handle error when pushing to list', async () => {
        const params = { serverUrl, database, key, value: 'task1', pushLeft: false };
        const error = createMockError('List error');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('redis/pushListElement', params)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('RedisStore', 'pushing list element', error);
      });

      it('should pop element from list successfully', async () => {
        const params = { serverUrl, database, key, popLeft: false };
        apiClient.post.mockResolvedValue({ data: 'task1' });

        const result = await store.dispatch('redis/popListElement', params);

        expect(apiClient.post).toHaveBeenCalledWith('/redis/list/pop', params);
        expect(logger.success).toHaveBeenCalledWith('Element popped from list successfully');
        expect(result).toBe('task1');
      });

      it('should show empty message when popping from empty list', async () => {
        const params = { serverUrl, database, key, popLeft: false };
        apiClient.post.mockResolvedValue({ data: null });

        const result = await store.dispatch('redis/popListElement', params);

        expect(logger.success).toHaveBeenCalledWith('List is empty');
        expect(result).toBe(null);
      });

      it('should get list elements successfully', async () => {
        const params = { serverUrl, database, key };
        apiClient.get.mockResolvedValue({ data: mockRedisListValue.values });

        const result = await store.dispatch('redis/getListElements', params);

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/list/${serverUrl}/${database}/${key}`);
        expect(result).toEqual(mockRedisListValue.values);
        const storeKey = `${serverUrl}_${database}_${key}`;
        expect(store.state.redis.redisListElements[storeKey]).toEqual(mockRedisListValue.values);
      });

      it('should get list length successfully', async () => {
        apiClient.get.mockResolvedValue({ data: 10 });

        const result = await store.dispatch('redis/getListLength', { serverUrl, database, key });

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/list/${serverUrl}/${database}/${key}/length`);
        expect(result).toBe(10);
      });

      it('should delete list successfully', async () => {
        apiClient.delete.mockResolvedValue({ data: {} });

        await store.dispatch('redis/deleteList', { serverUrl, database, key });

        expect(apiClient.delete).toHaveBeenCalledWith(`/redis/list/${serverUrl}/${database}/${key}`);
        expect(logger.success).toHaveBeenCalledWith('List deleted successfully');
      });

      it('should get all list keys successfully', async () => {
        const keys = ['list:1', 'list:2'];
        apiClient.get.mockResolvedValue({ data: keys });

        const result = await store.dispatch('redis/getAllListKeys', { serverUrl, database });

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/list/${serverUrl}/${database}`);
        expect(result).toEqual(keys);
        const storeKey = `${serverUrl}_${database}`;
        expect(store.state.redis.redisListKeys[storeKey]).toEqual(keys);
      });
    });

    // Hash Operations
    describe('Hash Operations', () => {
      const serverUrl = 'localhost:6379';
      const database = 0;
      const key = 'user:profile:1';

      it('should set hash field successfully', async () => {
        const params = { serverUrl, database, key, field: 'name', value: 'John' };
        apiClient.post.mockResolvedValue({ data: 1 });

        const result = await store.dispatch('redis/setHashField', params);

        expect(apiClient.post).toHaveBeenCalledWith('/redis/hash/field', params);
        expect(logger.success).toHaveBeenCalledWith('Hash field set successfully');
        expect(result).toBe(1);
      });

      it('should handle error when setting hash field', async () => {
        const params = { serverUrl, database, key, field: 'name', value: 'John' };
        const error = createMockError('Hash error');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('redis/setHashField', params)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('RedisStore', 'setting hash field', error);
      });

      it('should get hash field successfully', async () => {
        const params = { serverUrl, database, key, field: 'name' };
        apiClient.get.mockResolvedValue({ data: 'John' });

        const result = await store.dispatch('redis/getHashField', params);

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/hash/${serverUrl}/${database}/${key}/field/name`);
        expect(result).toBe('John');
      });

      it('should get all hash fields successfully', async () => {
        const params = { serverUrl, database, key };
        apiClient.get.mockResolvedValue({ data: mockRedisHashValue.fields });

        const result = await store.dispatch('redis/getHashAllFields', params);

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/hash/${serverUrl}/${database}/${key}`);
        expect(result).toEqual(mockRedisHashValue.fields);
        const storeKey = `${serverUrl}_${database}_${key}`;
        expect(store.state.redis.redisHashFields[storeKey]).toEqual(mockRedisHashValue.fields);
      });

      it('should delete hash field successfully', async () => {
        const params = { serverUrl, database, key, field: 'name' };
        apiClient.delete.mockResolvedValue({ data: {} });

        await store.dispatch('redis/deleteHashField', params);

        expect(apiClient.delete).toHaveBeenCalledWith(`/redis/hash/${serverUrl}/${database}/${key}/field/name`);
        expect(logger.success).toHaveBeenCalledWith('Hash field deleted successfully');
      });

      it('should delete hash successfully', async () => {
        apiClient.delete.mockResolvedValue({ data: {} });

        await store.dispatch('redis/deleteHash', { serverUrl, database, key });

        expect(apiClient.delete).toHaveBeenCalledWith(`/redis/hash/${serverUrl}/${database}/${key}`);
        expect(logger.success).toHaveBeenCalledWith('Hash deleted successfully');
      });

      it('should get all hash keys successfully', async () => {
        const keys = ['hash:1', 'hash:2'];
        apiClient.get.mockResolvedValue({ data: keys });

        const result = await store.dispatch('redis/getAllHashKeys', { serverUrl, database });

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/hash/${serverUrl}/${database}`);
        expect(result).toEqual(keys);
        const storeKey = `${serverUrl}_${database}`;
        expect(store.state.redis.redisHashKeys[storeKey]).toEqual(keys);
      });
    });

    // Set Operations
    describe('Set Operations', () => {
      const serverUrl = 'localhost:6379';
      const database = 0;
      const key = 'tags';

      it('should add set member successfully', async () => {
        const params = { serverUrl, database, key, member: 'javascript' };
        apiClient.post.mockResolvedValue({ data: 1 });

        const result = await store.dispatch('redis/addSetMember', params);

        expect(apiClient.post).toHaveBeenCalledWith('/redis/set/member', params);
        expect(logger.success).toHaveBeenCalledWith('Set member added successfully');
        expect(result).toBe(1);
      });

      it('should handle error when adding set member', async () => {
        const params = { serverUrl, database, key, member: 'javascript' };
        const error = createMockError('Set error');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('redis/addSetMember', params)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('RedisStore', 'adding set member', error);
      });

      it('should get set members successfully', async () => {
        const params = { serverUrl, database, key };
        apiClient.get.mockResolvedValue({ data: mockRedisSetValue.members });

        const result = await store.dispatch('redis/getSetMembers', params);

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/set/${serverUrl}/${database}/${key}`);
        expect(result).toEqual(mockRedisSetValue.members);
        const storeKey = `${serverUrl}_${database}_${key}`;
        expect(store.state.redis.redisSetMembers[storeKey]).toEqual(mockRedisSetValue.members);
      });

      it('should remove set member successfully', async () => {
        const params = { serverUrl, database, key, member: 'javascript' };
        apiClient.delete.mockResolvedValue({ data: {} });

        await store.dispatch('redis/removeSetMember', params);

        expect(apiClient.delete).toHaveBeenCalledWith(`/redis/set/${serverUrl}/${database}/${key}/member/javascript`);
        expect(logger.success).toHaveBeenCalledWith('Set member removed successfully');
      });

      it('should get set count successfully', async () => {
        apiClient.get.mockResolvedValue({ data: 5 });

        const result = await store.dispatch('redis/getSetCount', { serverUrl, database, key });

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/set/${serverUrl}/${database}/${key}/count`);
        expect(result).toBe(5);
      });

      it('should delete set successfully', async () => {
        apiClient.delete.mockResolvedValue({ data: {} });

        await store.dispatch('redis/deleteSet', { serverUrl, database, key });

        expect(apiClient.delete).toHaveBeenCalledWith(`/redis/set/${serverUrl}/${database}/${key}`);
        expect(logger.success).toHaveBeenCalledWith('Set deleted successfully');
      });

      it('should get all set keys successfully', async () => {
        const keys = ['set:1', 'set:2'];
        apiClient.get.mockResolvedValue({ data: keys });

        const result = await store.dispatch('redis/getAllSetKeys', { serverUrl, database });

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/set/${serverUrl}/${database}`);
        expect(result).toEqual(keys);
        const storeKey = `${serverUrl}_${database}`;
        expect(store.state.redis.redisSetKeys[storeKey]).toEqual(keys);
      });
    });

    // Sorted Set Operations
    describe('Sorted Set Operations', () => {
      const serverUrl = 'localhost:6379';
      const database = 0;
      const key = 'leaderboard';

      it('should add sorted set member successfully', async () => {
        const params = { serverUrl, database, key, member: 'player1', score: 100 };
        apiClient.post.mockResolvedValue({ data: 1 });

        const result = await store.dispatch('redis/addSortedSetMember', params);

        expect(apiClient.post).toHaveBeenCalledWith('/redis/sortedset/member', params);
        expect(logger.success).toHaveBeenCalledWith('Sorted set member added successfully');
        expect(result).toBe(1);
      });

      it('should handle error when adding sorted set member', async () => {
        const params = { serverUrl, database, key, member: 'player1', score: 100 };
        const error = createMockError('Sorted set error');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('redis/addSortedSetMember', params)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('RedisStore', 'adding sorted set member', error);
      });

      it('should get sorted set members successfully', async () => {
        const params = { serverUrl, database, key };
        apiClient.get.mockResolvedValue({ data: mockRedisSortedSetValue.members });

        const result = await store.dispatch('redis/getSortedSetMembers', params);

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/sortedset/${serverUrl}/${database}/${key}`);
        expect(result).toEqual(mockRedisSortedSetValue.members);
        const storeKey = `${serverUrl}_${database}_${key}`;
        expect(store.state.redis.redisSortedSetMembers[storeKey]).toEqual(mockRedisSortedSetValue.members);
      });

      it('should remove sorted set member successfully', async () => {
        const params = { serverUrl, database, key, member: 'player1' };
        apiClient.delete.mockResolvedValue({ data: {} });

        await store.dispatch('redis/removeSortedSetMember', params);

        expect(apiClient.delete).toHaveBeenCalledWith(`/redis/sortedset/${serverUrl}/${database}/${key}/member/player1`);
        expect(logger.success).toHaveBeenCalledWith('Sorted set member removed successfully');
      });

      it('should get sorted set count successfully', async () => {
        apiClient.get.mockResolvedValue({ data: 10 });

        const result = await store.dispatch('redis/getSortedSetCount', { serverUrl, database, key });

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/sortedset/${serverUrl}/${database}/${key}/count`);
        expect(result).toBe(10);
      });

      it('should delete sorted set successfully', async () => {
        apiClient.delete.mockResolvedValue({ data: {} });

        await store.dispatch('redis/deleteSortedSet', { serverUrl, database, key });

        expect(apiClient.delete).toHaveBeenCalledWith(`/redis/sortedset/${serverUrl}/${database}/${key}`);
        expect(logger.success).toHaveBeenCalledWith('Sorted set deleted successfully');
      });

      it('should get all sorted set keys successfully', async () => {
        const keys = ['sortedset:1', 'sortedset:2'];
        apiClient.get.mockResolvedValue({ data: keys });

        const result = await store.dispatch('redis/getAllSortedSetKeys', { serverUrl, database });

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/sortedset/${serverUrl}/${database}`);
        expect(result).toEqual(keys);
        const storeKey = `${serverUrl}_${database}`;
        expect(store.state.redis.redisSortedSetKeys[storeKey]).toEqual(keys);
      });
    });

    // Stream Operations
    describe('Stream Operations', () => {
      const serverUrl = 'localhost:6379';
      const database = 0;
      const key = 'events';

      it('should add stream entry successfully', async () => {
        const params = { serverUrl, database, key, fields: { event: 'login', user: 'john' } };
        apiClient.post.mockResolvedValue({ data: '1-0' });

        const result = await store.dispatch('redis/addStreamEntry', params);

        expect(apiClient.post).toHaveBeenCalledWith('/redis/stream/entry', params);
        expect(logger.success).toHaveBeenCalledWith('Stream entry added successfully');
        expect(result).toBe('1-0');
      });

      it('should handle error when adding stream entry', async () => {
        const params = { serverUrl, database, key, fields: { event: 'login' } };
        const error = createMockError('Stream error');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('redis/addStreamEntry', params)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('RedisStore', 'adding stream entry', error);
      });

      it('should get stream entries successfully', async () => {
        const params = { serverUrl, database, key, count: 50 };
        apiClient.get.mockResolvedValue({ data: mockRedisStreamValue.entries });

        const result = await store.dispatch('redis/getStreamEntries', params);

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/stream/${serverUrl}/${database}/${key}?count=50`);
        expect(result).toEqual(mockRedisStreamValue.entries);
        const storeKey = `${serverUrl}_${database}_${key}`;
        expect(store.state.redis.redisStreamEntries[storeKey]).toEqual(mockRedisStreamValue.entries);
      });

      it('should get stream entries with default count', async () => {
        const params = { serverUrl, database, key };
        apiClient.get.mockResolvedValue({ data: mockRedisStreamValue.entries });

        await store.dispatch('redis/getStreamEntries', params);

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/stream/${serverUrl}/${database}/${key}?count=100`);
      });

      it('should get stream length successfully', async () => {
        apiClient.get.mockResolvedValue({ data: 25 });

        const result = await store.dispatch('redis/getStreamLength', { serverUrl, database, key });

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/stream/${serverUrl}/${database}/${key}/length`);
        expect(result).toBe(25);
      });

      it('should delete stream successfully', async () => {
        apiClient.delete.mockResolvedValue({ data: {} });

        await store.dispatch('redis/deleteStream', { serverUrl, database, key });

        expect(apiClient.delete).toHaveBeenCalledWith(`/redis/stream/${serverUrl}/${database}/${key}`);
        expect(logger.success).toHaveBeenCalledWith('Stream deleted successfully');
      });

      it('should get all stream keys successfully', async () => {
        const keys = ['stream:1', 'stream:2'];
        apiClient.get.mockResolvedValue({ data: keys });

        const result = await store.dispatch('redis/getAllStreamKeys', { serverUrl, database });

        expect(apiClient.get).toHaveBeenCalledWith(`/redis/stream/${serverUrl}/${database}`);
        expect(result).toEqual(keys);
        const storeKey = `${serverUrl}_${database}`;
        expect(store.state.redis.redisStreamKeys[storeKey]).toEqual(keys);
      });
    });
  });
});
