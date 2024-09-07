/* eslint-disable no-unused-vars */
import apiClient from '@/services/apiClient';

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
        .catch(console.error);
    },
    getDatabaseDetails({ commit }, { serverUrl, database }) {
      apiClient.get(`/redis/${serverUrl}/${database}`)
        .then((result) => commit('updateDatabase', { serverUrl, database, messagesCount: result.data.messagesCount }))
        .catch(console.error);
    },
    async cacheString({ commit }, cacheObject) {
      await apiClient.post('/redis/string', cacheObject)
        .then(() => {})
        .catch(console.error);
    },
    getCachedString({ commit }, { serverUrl, database, key }) {
      apiClient.get(`/redis/string/${serverUrl}/${database}/${key}`)
        .then((result) => commit('updateStringCache', {
          serverUrl, database, key, value: result.data,
        }))
        .catch(console.error);
    },
    async invalidateCachedString({ commit }, { serverUrl, database, key }) {
      await apiClient.delete(`/redis/string/${serverUrl}/${database}/${key}`)
        .then(() => commit('invalidateStringCache', {
          serverUrl, database, key,
        }))
        .catch(console.error);
    },
    async getAllStringKeys({ commit }, { serverUrl, database }) {
      await apiClient.get(`/redis/string/${serverUrl}/${database}`)
        .then((result) => commit('updateDatabaseKeys', {
          serverUrl, database, keys: result.data,
        }))
        .catch(console.error);
    },
  },
};
