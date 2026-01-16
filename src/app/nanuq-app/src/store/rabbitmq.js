import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';

export default {
  namespaced: true,
  state: {
    exchanges: {},      // { serverUrl: [exchanges] }
    queues: {},         // { serverUrl: [queues] }
    queueDetails: {},   // { 'serverUrl-queueName': details }
  },
  getters: {
    getExchanges(state) {
      return (serverUrl) => state.exchanges[serverUrl] || [];
    },
    getQueues(state) {
      return (serverUrl) => state.queues[serverUrl] || [];
    },
    getQueueDetails(state) {
      return (serverUrl, queueName) => {
        const key = `${serverUrl}-${queueName}`;
        return state.queueDetails[key];
      };
    },
  },
  mutations: {
    updateExchanges(state, { serverUrl, data }) {
      delete state.exchanges[serverUrl];
      state.exchanges[serverUrl] = data;
    },
    updateQueues(state, { serverUrl, data }) {
      delete state.queues[serverUrl];
      state.queues[serverUrl] = data;
    },
    updateQueueDetails(state, { serverUrl, queueName, data }) {
      const key = `${serverUrl}-${queueName}`;
      delete state.queueDetails[key];
      state.queueDetails[key] = data;
    },
  },
  actions: {
    // Exchanges
    loadExchanges({ commit }, serverUrl) {
      apiClient.get(`/rabbitmq/exchanges/${serverUrl}`)
        .then((result) => commit('updateExchanges', { serverUrl, data: result.data }))
        .catch((error) => logger.handleApiError('RabbitMQStore', 'loading exchanges', error));
    },
    async addExchange({ commit }, exchangeDetails) {
      await apiClient.post('/rabbitmq/exchange', exchangeDetails)
        .then(() => logger.success('Exchange created successfully'))
        .catch((error) => logger.handleApiError('RabbitMQStore', 'adding exchange', error));
    },
    async deleteExchange({ commit }, { serverUrl, name }) {
      await apiClient.delete(`/rabbitmq/exchange/${serverUrl}/${name}`)
        .then(() => logger.success('Exchange deleted successfully'))
        .catch((error) => logger.handleApiError('RabbitMQStore', 'deleting exchange', error));
    },

    // Queues
    loadQueues({ commit }, serverUrl) {
      apiClient.get(`/rabbitmq/queues/${serverUrl}`)
        .then((result) => commit('updateQueues', { serverUrl, data: result.data }))
        .catch((error) => logger.handleApiError('RabbitMQStore', 'loading queues', error));
    },
    async loadQueueDetails({ commit }, { serverUrl, queueName }) {
      await apiClient.get(`/rabbitmq/queue/${serverUrl}/${queueName}`)
        .then((result) => commit('updateQueueDetails', { serverUrl, queueName, data: result.data }))
        .catch((error) => logger.handleApiError('RabbitMQStore', 'loading queue details', error));
    },
    async addQueue({ commit }, queueDetails) {
      await apiClient.post('/rabbitmq/queue', queueDetails)
        .then(() => logger.success('Queue created successfully'))
        .catch((error) => logger.handleApiError('RabbitMQStore', 'adding queue', error));
    },
    async deleteQueue({ commit }, { serverUrl, name }) {
      await apiClient.delete(`/rabbitmq/queue/${serverUrl}/${name}`)
        .then(() => logger.success('Queue deleted successfully'))
        .catch((error) => logger.handleApiError('RabbitMQStore', 'deleting queue', error));
    },
  },
};
