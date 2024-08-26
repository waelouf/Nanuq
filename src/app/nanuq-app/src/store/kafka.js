import axios from 'axios';

export default {
  namespaced: true,
  state: {
    kafkaTopics: {},
  },
  getters: {
  },
  mutations: {
    updateTopics(state, topicsObject) {
      delete state.kafkaTopics[topicsObject.serverName];
      state.kafkaTopics[topicsObject.serverName] = topicsObject.data;
      console.log(topicsObject);
    },
  },
  actions: {
    loadKafkaTopics({ commit }, serverName) {
      axios.get(`/kafka/topic/${serverName}`)
        .then((result) => commit('updateTopics', { data: result.data, serverName }));
    },
  },
};
