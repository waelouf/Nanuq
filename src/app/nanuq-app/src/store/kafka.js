/* eslint-disable no-unused-vars */
import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';

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
      apiClient.get(`/kafka/topic/${serverName}`)
        .then((result) => commit('updateTopics', { data: result.data, serverName }));
    },
    async loadKafkaTopicDetails({ commit }, { serverName, topicName }) {
      await apiClient.get(`/kafka/topic/${serverName}/${topicName}`)
        .then((result) => commit('updateTopicDetails', { data: result.data, serverName, topicName }));
    },
    async addKafkaTopic({ commit }, topicDetails) {
      await apiClient.post('kafka/topic', topicDetails)
        .then(() => {})
        .catch((error) => logger.handleApiError('KafkaStore', 'adding Kafka topic', error));
    },
    async deleteKafkaTopic({ commit }, { bootstrapServer, topicName }) {
      await apiClient.delete(`kafka/topic/${bootstrapServer}/${topicName}`)
        .then(() => {})
        .catch((error) => logger.handleApiError('KafkaStore', 'deleting Kafka topic', error));
    },
  },
};
