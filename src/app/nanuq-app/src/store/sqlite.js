import apiClient from '@/services/apiClient';

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
        .catch(console.error);
    },
    // eslint-disable-next-line no-unused-vars
    addKafkaServer({ commit }, serverDetails) {
      apiClient.post('/sqlite/kafka', serverDetails)
        .then(() => {})
        .catch(console.error);
    },
    // eslint-disable-next-line no-unused-vars
    async deleteKafkaServer({ commit }, id) {
      await apiClient.delete(`/sqlite/kafka/${id}`)
        .then(() => {})
        .catch(console.error);
    },
    // Redis
    loadRedisServers({ commit }) {
      apiClient.get('/sqlite/redis')
        .then((result) => commit('loadRedisServers', result.data))
        .catch(console.error);
    },
    // eslint-disable-next-line no-unused-vars
    async addRedisServer({ commit }, serverDetails) {
      await apiClient.post('/sqlite/redis', serverDetails)
        .then(() => {})
        .catch(console.error);
    },
    // eslint-disable-next-line no-unused-vars
    async deleteRedisServer({ commit }, id) {
      await apiClient.delete(`/sqlite/redis/${id}`)
        .then(() => {})
        .catch(console.error);
    },
  },
  modules: {
  },
};
