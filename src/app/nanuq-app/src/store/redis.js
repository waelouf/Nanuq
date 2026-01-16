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
  },
  getters: {
    getStringCache(state, key) {
      return state.redisCachedStrings[key];
    },
    getDatabaseCachedKeys(state, key) {
      return state.redisDatabaseKeys[key];
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
  },
};
