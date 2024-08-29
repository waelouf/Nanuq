import axios from 'axios';

export default {
  namespaced: true,
  state: {
    kafkaTopics: {},
    kafkaTopicDetails: {},
  },
  getters: {
    getTopicNumberOfMessages(state, key) {
      return state.kafkaTopicDetails[key];
    },
  },
  mutations: {
    updateTopics(state, topicsObject) {
      delete state.kafkaTopics[topicsObject.serverName];
      state.kafkaTopics[topicsObject.serverName] = topicsObject.data;
    },
    updateTopicDetails(state, { data, serverName, topicName }) {
      const key = `${serverName}-${topicName}`;
      delete state.kafkaTopicDetails[key];
      state.kafkaTopicDetails[key] = data.numberOfMessages;
    },
  },
  actions: {
    loadKafkaTopics({ commit }, serverName) {
      axios.get(`/kafka/topic/${serverName}`)
        .then((result) => commit('updateTopics', { data: result.data, serverName }));
    },
    async loadKafkaTopicDetails({ commit }, { serverName, topicName }) {
      await axios.get(`/kafka/topic/${serverName}/${topicName}`)
        .then((result) => commit('updateTopicDetails', { data: result.data, serverName, topicName }));
    },
  },
};
