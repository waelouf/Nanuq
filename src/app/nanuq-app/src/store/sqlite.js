import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';

export default {
  namespaced: true,
  state: {
    kafkaServers: [],
    redisServers: [],
    rabbitMQServers: [],
    awsServers: [],
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
    loadRabbitMQServers(state, servers) {
      state.rabbitMQServers = servers;
    },
    loadAwsServers(state, servers) {
      state.awsServers = servers;
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
    async addKafkaServer({ commit }, serverDetails) {
      try {
        const result = await apiClient.post('/sqlite/kafka', serverDetails);
        logger.success('Kafka server added successfully');
        return result.data; // Return the server ID
      } catch (error) {
        logger.handleApiError('SQLiteStore', 'adding Kafka server', error);
        throw error;
      }
    },
    // eslint-disable-next-line no-unused-vars
    async deleteKafkaServer({ commit }, id) {
      await apiClient.delete(`/sqlite/kafka/${id}`)
        .then(() => logger.success('Kafka server deleted successfully'))
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
      try {
        const result = await apiClient.post('/sqlite/redis', serverDetails);
        logger.success('Redis server added successfully');
        return result.data; // Return the server ID
      } catch (error) {
        logger.handleApiError('SQLiteStore', 'adding Redis server', error);
        throw error;
      }
    },
    // eslint-disable-next-line no-unused-vars
    async deleteRedisServer({ commit }, id) {
      await apiClient.delete(`/sqlite/redis/${id}`)
        .then(() => logger.success('Redis server deleted successfully'))
        .catch((error) => logger.handleApiError('SQLiteStore', 'deleting Redis server', error));
    },
    // RabbitMQ
    loadRabbitMQServers({ commit }) {
      apiClient.get('/sqlite/rabbitmq')
        .then((result) => commit('loadRabbitMQServers', result.data))
        .catch((error) => logger.handleApiError('SQLiteStore', 'loading RabbitMQ servers', error));
    },
    // eslint-disable-next-line no-unused-vars
    async addRabbitMQServer({ commit }, serverDetails) {
      try {
        const result = await apiClient.post('/sqlite/rabbitmq', serverDetails);
        logger.success('RabbitMQ server added successfully');
        return result.data; // Return the server ID
      } catch (error) {
        logger.handleApiError('SQLiteStore', 'adding RabbitMQ server', error);
        throw error;
      }
    },
    // eslint-disable-next-line no-unused-vars
    async deleteRabbitMQServer({ commit }, id) {
      await apiClient.delete(`/sqlite/rabbitmq/${id}`)
        .then(() => logger.success('RabbitMQ server deleted successfully'))
        .catch((error) => logger.handleApiError('SQLiteStore', 'deleting RabbitMQ server', error));
    },
    // AWS
    loadAwsServers({ commit }) {
      apiClient.get('/sqlite/aws')
        .then((result) => commit('loadAwsServers', result.data))
        .catch((error) => logger.handleApiError('SQLiteStore', 'loading AWS servers', error));
    },
    // eslint-disable-next-line no-unused-vars
    async addAwsServer({ commit }, serverDetails) {
      try {
        const result = await apiClient.post('/sqlite/aws', serverDetails);
        logger.success('AWS server added successfully');
        return result.data; // Return the server ID
      } catch (error) {
        logger.handleApiError('SQLiteStore', 'adding AWS server', error);
        throw error;
      }
    },
    // eslint-disable-next-line no-unused-vars
    async deleteAwsServer({ commit }, id) {
      await apiClient.delete(`/sqlite/aws/${id}`)
        .then(() => logger.success('AWS server deleted successfully'))
        .catch((error) => logger.handleApiError('SQLiteStore', 'deleting AWS server', error));
    },
  },
  modules: {
  },
};
