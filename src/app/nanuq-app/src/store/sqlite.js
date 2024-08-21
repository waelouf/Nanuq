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
      axios.get('/kafka')
        .then((result) => commit('loadKafkaServers', result.data))
        .catch(console.error);
    },
  },
  modules: {
  },
};
