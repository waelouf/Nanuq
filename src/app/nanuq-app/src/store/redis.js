/* eslint-disable no-unused-vars */
import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';

export default {
  namespaced: true,
  state: {
    redisServers: {},
    redisDatabaseMessageCounter: {},
    redisCachedStrings: {},
    redisDatabaseKeys: {},
    redisListElements: {},
    redisListKeys: {},
    redisHashFields: {},
    redisHashKeys: {},
    redisSetMembers: {},
    redisSetKeys: {},
    redisSortedSetMembers: {},
    redisSortedSetKeys: {},
    redisStreamEntries: {},
    redisStreamKeys: {},
  },
  getters: {
    getStringCache(state, key) {
      return state.redisCachedStrings[key];
    },
    getDatabaseCachedKeys(state, key) {
      return state.redisDatabaseKeys[key];
    },
    getListElements(state, key) {
      return state.redisListElements[key];
    },
    getDatabaseListKeys(state, key) {
      return state.redisListKeys[key];
    },
    getHashFields(state, key) {
      return state.redisHashFields[key];
    },
    getDatabaseHashKeys(state, key) {
      return state.redisHashKeys[key];
    },
    getSetMembers(state, key) {
      return state.redisSetMembers[key];
    },
    getDatabaseSetKeys(state, key) {
      return state.redisSetKeys[key];
    },
    getSortedSetMembers(state, key) {
      return state.redisSortedSetMembers[key];
    },
    getDatabaseSortedSetKeys(state, key) {
      return state.redisSortedSetKeys[key];
    },
    getStreamEntries(state, key) {
      return state.redisStreamEntries[key];
    },
    getDatabaseStreamKeys(state, key) {
      return state.redisStreamKeys[key];
    },
  },
  mutations: {
    updateServers(state, serverObject) {
      delete state.redisServers[serverObject.serverUrl];
      state.redisServers[serverObject.serverUrl] = serverObject.serverDetails;
    },
    updateDatabase(state, databaseObject) {
      const key = `${databaseObject.serverUrl}_${databaseObject.database}`;
      delete state.redisDatabaseMessageCounter[key];
      state.redisDatabaseMessageCounter[key] = databaseObject.messagesCount;
    },
    updateStringCache(state, cacheObject) {
      const key = `${cacheObject.serverUrl}_${cacheObject.database}_${cacheObject.key}`;
      delete state.redisCachedStrings[key];
      state.redisCachedStrings[key] = cacheObject.value;
    },
    invalidateStringCache(state, invalidateCacheObject) {
      const key = `${invalidateCacheObject.serverUrl}_${invalidateCacheObject.database}_${invalidateCacheObject.key}`;
      delete state.redisCachedStrings[key];
    },
    updateDatabaseKeys(state, allKeysObject) {
      const key = `${allKeysObject.serverUrl}_${allKeysObject.database}`;
      delete state.redisDatabaseKeys[key];
      state.redisDatabaseKeys[key] = allKeysObject.keys;
    },
    updateListElements(state, listObject) {
      const key = `${listObject.serverUrl}_${listObject.database}_${listObject.key}`;
      delete state.redisListElements[key];
      state.redisListElements[key] = listObject.elements;
    },
    invalidateListElements(state, invalidateListObject) {
      const key = `${invalidateListObject.serverUrl}_${invalidateListObject.database}_${invalidateListObject.key}`;
      delete state.redisListElements[key];
    },
    updateDatabaseListKeys(state, allListKeysObject) {
      const key = `${allListKeysObject.serverUrl}_${allListKeysObject.database}`;
      delete state.redisListKeys[key];
      state.redisListKeys[key] = allListKeysObject.keys;
    },
    updateHashFields(state, hashObject) {
      const key = `${hashObject.serverUrl}_${hashObject.database}_${hashObject.key}`;
      delete state.redisHashFields[key];
      state.redisHashFields[key] = hashObject.fields;
    },
    invalidateHashFields(state, invalidateHashObject) {
      const key = `${invalidateHashObject.serverUrl}_${invalidateHashObject.database}_${invalidateHashObject.key}`;
      delete state.redisHashFields[key];
    },
    updateDatabaseHashKeys(state, allHashKeysObject) {
      const key = `${allHashKeysObject.serverUrl}_${allHashKeysObject.database}`;
      delete state.redisHashKeys[key];
      state.redisHashKeys[key] = allHashKeysObject.keys;
    },
    updateSetMembers(state, setObject) {
      const key = `${setObject.serverUrl}_${setObject.database}_${setObject.key}`;
      delete state.redisSetMembers[key];
      state.redisSetMembers[key] = setObject.members;
    },
    invalidateSetMembers(state, invalidateSetObject) {
      const key = `${invalidateSetObject.serverUrl}_${invalidateSetObject.database}_${invalidateSetObject.key}`;
      delete state.redisSetMembers[key];
    },
    updateDatabaseSetKeys(state, allSetKeysObject) {
      const key = `${allSetKeysObject.serverUrl}_${allSetKeysObject.database}`;
      delete state.redisSetKeys[key];
      state.redisSetKeys[key] = allSetKeysObject.keys;
    },
    updateSortedSetMembers(state, sortedSetObject) {
      const key = `${sortedSetObject.serverUrl}_${sortedSetObject.database}_${sortedSetObject.key}`;
      delete state.redisSortedSetMembers[key];
      state.redisSortedSetMembers[key] = sortedSetObject.members;
    },
    invalidateSortedSetMembers(state, invalidateSortedSetObject) {
      const key = `${invalidateSortedSetObject.serverUrl}_${invalidateSortedSetObject.database}_${invalidateSortedSetObject.key}`;
      delete state.redisSortedSetMembers[key];
    },
    updateDatabaseSortedSetKeys(state, allSortedSetKeysObject) {
      const key = `${allSortedSetKeysObject.serverUrl}_${allSortedSetKeysObject.database}`;
      delete state.redisSortedSetKeys[key];
      state.redisSortedSetKeys[key] = allSortedSetKeysObject.keys;
    },
    updateStreamEntries(state, streamObject) {
      const key = `${streamObject.serverUrl}_${streamObject.database}_${streamObject.key}`;
      delete state.redisStreamEntries[key];
      state.redisStreamEntries[key] = streamObject.entries;
    },
    invalidateStreamEntries(state, invalidateStreamObject) {
      const key = `${invalidateStreamObject.serverUrl}_${invalidateStreamObject.database}_${invalidateStreamObject.key}`;
      delete state.redisStreamEntries[key];
    },
    updateDatabaseStreamKeys(state, allStreamKeysObject) {
      const key = `${allStreamKeysObject.serverUrl}_${allStreamKeysObject.database}`;
      delete state.redisStreamKeys[key];
      state.redisStreamKeys[key] = allStreamKeysObject.keys;
    },
  },
  actions: {
    async getServerDetails({ commit }, serverUrl) {
      await apiClient.get(`/redis/${serverUrl}`)
        .then((result) => commit('updateServers', { serverUrl, serverDetails: result.data }))
        .catch((error) => logger.handleApiError('RedisStore', 'getting server details', error));
    },
    getDatabaseDetails({ commit }, { serverUrl, database }) {
      apiClient.get(`/redis/${serverUrl}/${database}`)
        .then((result) => commit('updateDatabase', { serverUrl, database, messagesCount: result.data.messagesCount }))
        .catch((error) => logger.handleApiError('RedisStore', 'getting database details', error));
    },
    async cacheString({ commit }, cacheObject) {
      await apiClient.post('/redis/string', cacheObject)
        .then(() => logger.success('String cached successfully'))
        .catch((error) => logger.handleApiError('RedisStore', 'caching string', error));
    },
    getCachedString({ commit }, { serverUrl, database, key }) {
      apiClient.get(`/redis/string/${serverUrl}/${database}/${key}`)
        .then((result) => commit('updateStringCache', {
          serverUrl, database, key, value: result.data,
        }))
        .catch((error) => logger.handleApiError('RedisStore', 'getting cached string', error));
    },
    async invalidateCachedString({ commit }, { serverUrl, database, key }) {
      await apiClient.delete(`/redis/string/${serverUrl}/${database}/${key}`)
        .then(() => {
          commit('invalidateStringCache', {
            serverUrl, database, key,
          });
          logger.success('Cached string invalidated successfully');
        })
        .catch((error) => logger.handleApiError('RedisStore', 'invalidating cached string', error));
    },
    async getAllStringKeys({ commit }, { serverUrl, database }) {
      await apiClient.get(`/redis/string/${serverUrl}/${database}`)
        .then((result) => commit('updateDatabaseKeys', {
          serverUrl, database, keys: result.data,
        }))
        .catch((error) => logger.handleApiError('RedisStore', 'getting all string keys', error));
    },
    // List operations
    async pushListElement({ commit }, { serverUrl, database, key, value, pushLeft }) {
      try {
        const result = await apiClient.post('/redis/list/push', {
          serverUrl,
          database,
          key,
          value,
          pushLeft,
        });
        logger.success(`Element pushed to list successfully (length: ${result.data})`);
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'pushing list element', error);
        throw error;
      }
    },
    async popListElement({ commit }, { serverUrl, database, key, popLeft }) {
      try {
        const result = await apiClient.post('/redis/list/pop', {
          serverUrl,
          database,
          key,
          popLeft,
        });
        if (result.data) {
          logger.success('Element popped from list successfully');
        } else {
          logger.success('List is empty');
        }
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'popping list element', error);
        throw error;
      }
    },
    async getListElements({ commit }, { serverUrl, database, key }) {
      try {
        const result = await apiClient.get(`/redis/list/${serverUrl}/${database}/${key}`);
        commit('updateListElements', {
          serverUrl,
          database,
          key,
          elements: result.data,
        });
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting list elements', error);
        throw error;
      }
    },
    async getListLength({ commit }, { serverUrl, database, key }) {
      try {
        const result = await apiClient.get(`/redis/list/${serverUrl}/${database}/${key}/length`);
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting list length', error);
        throw error;
      }
    },
    async deleteList({ commit }, { serverUrl, database, key }) {
      try {
        await apiClient.delete(`/redis/list/${serverUrl}/${database}/${key}`);
        commit('invalidateListElements', {
          serverUrl,
          database,
          key,
        });
        logger.success('List deleted successfully');
      } catch (error) {
        logger.handleApiError('RedisStore', 'deleting list', error);
        throw error;
      }
    },
    async getAllListKeys({ commit }, { serverUrl, database }) {
      try {
        const result = await apiClient.get(`/redis/list/${serverUrl}/${database}`);
        commit('updateDatabaseListKeys', {
          serverUrl,
          database,
          keys: result.data,
        });
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting all list keys', error);
        throw error;
      }
    },
    // Hash operations
    async setHashField({ commit }, { serverUrl, database, key, field, value }) {
      try {
        const result = await apiClient.post('/redis/hash/field', {
          serverUrl,
          database,
          key,
          field,
          value,
        });
        logger.success('Hash field set successfully');
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'setting hash field', error);
        throw error;
      }
    },
    async getHashField({ commit }, { serverUrl, database, key, field }) {
      try {
        const result = await apiClient.get(`/redis/hash/${serverUrl}/${database}/${key}/field/${field}`);
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting hash field', error);
        throw error;
      }
    },
    async getHashAllFields({ commit }, { serverUrl, database, key }) {
      try {
        const result = await apiClient.get(`/redis/hash/${serverUrl}/${database}/${key}`);
        commit('updateHashFields', {
          serverUrl,
          database,
          key,
          fields: result.data,
        });
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting hash fields', error);
        throw error;
      }
    },
    async deleteHashField({ commit }, { serverUrl, database, key, field }) {
      try {
        await apiClient.delete(`/redis/hash/${serverUrl}/${database}/${key}/field/${field}`);
        logger.success('Hash field deleted successfully');
      } catch (error) {
        logger.handleApiError('RedisStore', 'deleting hash field', error);
        throw error;
      }
    },
    async deleteHash({ commit }, { serverUrl, database, key }) {
      try {
        await apiClient.delete(`/redis/hash/${serverUrl}/${database}/${key}`);
        commit('invalidateHashFields', {
          serverUrl,
          database,
          key,
        });
        logger.success('Hash deleted successfully');
      } catch (error) {
        logger.handleApiError('RedisStore', 'deleting hash', error);
        throw error;
      }
    },
    async getAllHashKeys({ commit }, { serverUrl, database }) {
      try {
        const result = await apiClient.get(`/redis/hash/${serverUrl}/${database}`);
        commit('updateDatabaseHashKeys', {
          serverUrl,
          database,
          keys: result.data,
        });
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting all hash keys', error);
        throw error;
      }
    },
    // Set operations
    async addSetMember({ commit }, { serverUrl, database, key, member }) {
      try {
        const result = await apiClient.post('/redis/set/member', {
          serverUrl,
          database,
          key,
          member,
        });
        logger.success('Set member added successfully');
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'adding set member', error);
        throw error;
      }
    },
    async getSetMembers({ commit }, { serverUrl, database, key }) {
      try {
        const result = await apiClient.get(`/redis/set/${serverUrl}/${database}/${key}`);
        commit('updateSetMembers', {
          serverUrl,
          database,
          key,
          members: result.data,
        });
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting set members', error);
        throw error;
      }
    },
    async removeSetMember({ commit }, { serverUrl, database, key, member }) {
      try {
        await apiClient.delete(`/redis/set/${serverUrl}/${database}/${key}/member/${member}`);
        logger.success('Set member removed successfully');
      } catch (error) {
        logger.handleApiError('RedisStore', 'removing set member', error);
        throw error;
      }
    },
    async getSetCount({ commit }, { serverUrl, database, key }) {
      try {
        const result = await apiClient.get(`/redis/set/${serverUrl}/${database}/${key}/count`);
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting set count', error);
        throw error;
      }
    },
    async deleteSet({ commit }, { serverUrl, database, key }) {
      try {
        await apiClient.delete(`/redis/set/${serverUrl}/${database}/${key}`);
        commit('invalidateSetMembers', {
          serverUrl,
          database,
          key,
        });
        logger.success('Set deleted successfully');
      } catch (error) {
        logger.handleApiError('RedisStore', 'deleting set', error);
        throw error;
      }
    },
    async getAllSetKeys({ commit }, { serverUrl, database }) {
      try {
        const result = await apiClient.get(`/redis/set/${serverUrl}/${database}`);
        commit('updateDatabaseSetKeys', {
          serverUrl,
          database,
          keys: result.data,
        });
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting all set keys', error);
        throw error;
      }
    },
    // Sorted Set operations
    async addSortedSetMember({ commit }, { serverUrl, database, key, member, score }) {
      try {
        const result = await apiClient.post('/redis/sortedset/member', {
          serverUrl,
          database,
          key,
          member,
          score,
        });
        logger.success('Sorted set member added successfully');
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'adding sorted set member', error);
        throw error;
      }
    },
    async getSortedSetMembers({ commit }, { serverUrl, database, key }) {
      try {
        const result = await apiClient.get(`/redis/sortedset/${serverUrl}/${database}/${key}`);
        commit('updateSortedSetMembers', {
          serverUrl,
          database,
          key,
          members: result.data,
        });
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting sorted set members', error);
        throw error;
      }
    },
    async removeSortedSetMember({ commit }, { serverUrl, database, key, member }) {
      try {
        await apiClient.delete(`/redis/sortedset/${serverUrl}/${database}/${key}/member/${member}`);
        logger.success('Sorted set member removed successfully');
      } catch (error) {
        logger.handleApiError('RedisStore', 'removing sorted set member', error);
        throw error;
      }
    },
    async getSortedSetCount({ commit }, { serverUrl, database, key }) {
      try {
        const result = await apiClient.get(`/redis/sortedset/${serverUrl}/${database}/${key}/count`);
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting sorted set count', error);
        throw error;
      }
    },
    async deleteSortedSet({ commit }, { serverUrl, database, key }) {
      try {
        await apiClient.delete(`/redis/sortedset/${serverUrl}/${database}/${key}`);
        commit('invalidateSortedSetMembers', {
          serverUrl,
          database,
          key,
        });
        logger.success('Sorted set deleted successfully');
      } catch (error) {
        logger.handleApiError('RedisStore', 'deleting sorted set', error);
        throw error;
      }
    },
    async getAllSortedSetKeys({ commit }, { serverUrl, database }) {
      try {
        const result = await apiClient.get(`/redis/sortedset/${serverUrl}/${database}`);
        commit('updateDatabaseSortedSetKeys', {
          serverUrl,
          database,
          keys: result.data,
        });
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting all sorted set keys', error);
        throw error;
      }
    },
    // Stream operations
    async addStreamEntry({ commit }, { serverUrl, database, key, fields }) {
      try {
        const result = await apiClient.post('/redis/stream/entry', {
          serverUrl,
          database,
          key,
          fields,
        });
        logger.success('Stream entry added successfully');
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'adding stream entry', error);
        throw error;
      }
    },
    async getStreamEntries({ commit }, { serverUrl, database, key, count = 100 }) {
      try {
        const result = await apiClient.get(`/redis/stream/${serverUrl}/${database}/${key}?count=${count}`);
        commit('updateStreamEntries', {
          serverUrl,
          database,
          key,
          entries: result.data,
        });
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting stream entries', error);
        throw error;
      }
    },
    async getStreamLength({ commit }, { serverUrl, database, key }) {
      try {
        const result = await apiClient.get(`/redis/stream/${serverUrl}/${database}/${key}/length`);
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting stream length', error);
        throw error;
      }
    },
    async deleteStream({ commit }, { serverUrl, database, key }) {
      try {
        await apiClient.delete(`/redis/stream/${serverUrl}/${database}/${key}`);
        commit('invalidateStreamEntries', {
          serverUrl,
          database,
          key,
        });
        logger.success('Stream deleted successfully');
      } catch (error) {
        logger.handleApiError('RedisStore', 'deleting stream', error);
        throw error;
      }
    },
    async getAllStreamKeys({ commit }, { serverUrl, database }) {
      try {
        const result = await apiClient.get(`/redis/stream/${serverUrl}/${database}`);
        commit('updateDatabaseStreamKeys', {
          serverUrl,
          database,
          keys: result.data,
        });
        return result.data;
      } catch (error) {
        logger.handleApiError('RedisStore', 'getting all stream keys', error);
        throw error;
      }
    },
  },
};
