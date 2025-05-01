import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';

export default {
  namespaced: true,
  state: {
    kafkaServers: [],
    redisServers: [],
  },
  getters: {
  },
  mutations: {
    loadKafkaServers(state, servers) {
      state.kafkaServers = servers;
    },
    loadRedisServers(state, servers) {
      state.redisServers = servers;
    },
  },
  actions: {
    // Kafka
    loadKafkaServers({ commit }) {
      apiClient.get('/sqlite/kafka')
        .then((result) => commit('loadKafkaServers', result.data))
        .catch((error) => logger.handleApiError('SQLiteStore', 'loading Kafka servers', error));
    },
    // eslint-disable-next-line no-unused-vars
    addKafkaServer({ commit }, serverDetails) {
      apiClient.post('/sqlite/kafka', serverDetails)
        .then(() => {})
        .catch((error) => logger.handleApiError('SQLiteStore', 'adding Kafka server', error));
    },
    // eslint-disable-next-line no-unused-vars
    async deleteKafkaServer({ commit }, id) {
      await apiClient.delete(`/sqlite/kafka/${id}`)
        .then(() => {})
        .catch((error) => logger.handleApiError('SQLiteStore', 'deleting Kafka server', error));
    },
    // Redis
    loadRedisServers({ commit }) {
      apiClient.get('/sqlite/redis')
        .then((result) => commit('loadRedisServers', result.data))
        .catch((error) => logger.handleApiError('SQLiteStore', 'loading Redis servers', error));
    },
    // eslint-disable-next-line no-unused-vars
    async addRedisServer({ commit }, serverDetails) {
      await apiClient.post('/sqlite/redis', serverDetails)
        .then(() => {})
        .catch((error) => logger.handleApiError('SQLiteStore', 'adding Redis server', error));
    },
    // eslint-disable-next-line no-unused-vars
    async deleteRedisServer({ commit }, id) {
      await apiClient.delete(`/sqlite/redis/${id}`)
        .then(() => {})
        .catch((error) => logger.handleApiError('SQLiteStore', 'deleting Redis server', error));
    },
  },
  modules: {
  },
};
