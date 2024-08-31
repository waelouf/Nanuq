import axios from 'axios';

export default {
  namespaced: true,
  state: {
    kafkaServers: [],
  },
  getters: {
  },
  mutations: {
    loadKafkaServers(state, servers) {
      state.kafkaServers = servers;
    },
  },
  actions: {
    loadKafkaServers({ commit }) {
      axios.get('/sqlite/kafka')
        .then((result) => commit('loadKafkaServers', result.data))
        .catch(console.error);
    },
    // eslint-disable-next-line no-unused-vars
    addKafkaServer({ commit }, serverDetails) {
      axios.post('/sqlite/kafka', serverDetails)
        .then(() => {})
        .catch(console.error);
    },
    // eslint-disable-next-line no-unused-vars
    async deleteKafkaServer({ commit }, id) {
      await axios.delete(`/sqlite/kafka/${id}`)
        .then(() => {})
        .catch(console.error);
    },
  },
  modules: {
  },
};
