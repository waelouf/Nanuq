import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';

export default {
  namespaced: true,
  state: {
    queues: [],
    topics: [],
    subscriptions: [],
    loading: false,
    error: null,
  },
  mutations: {
    setQueues(state, queues) {
      state.queues = queues;
    },
    setTopics(state, topics) {
      state.topics = topics;
    },
    setSubscriptions(state, subscriptions) {
      state.subscriptions = subscriptions;
    },
    setLoading(state, isLoading) {
      state.loading = isLoading;
    },
    setError(state, error) {
      state.error = error;
    },
    clearError(state) {
      state.error = null;
    },
  },
  actions: {
    // QUEUE ACTIONS
    async loadQueues({ commit }, serverId) {
      try {
        commit('setLoading', true);
        commit('clearError');
        const response = await apiClient.get(`/azure/servicebus/queues/${serverId}`);
        commit('setQueues', response.data);
      } catch (error) {
        commit('setError', 'Failed to load queues');
        logger.handleApiError('AzureStore', 'loading queues', error);
        throw error;
      } finally {
        commit('setLoading', false);
      }
    },

    async createQueue({ dispatch }, queueDetails) {
      try {
        await apiClient.post('/azure/servicebus/queue', queueDetails);
        logger.showSuccess('Queue created successfully');
        await dispatch('loadQueues', queueDetails.serverId);
      } catch (error) {
        logger.handleApiError('AzureStore', 'creating queue', error);
        throw error;
      }
    },

    async deleteQueue({ dispatch }, { serverId, queueName }) {
      try {
        await apiClient.delete('/azure/servicebus/queue', {
          data: { serverId, queueName },
        });
        logger.showSuccess('Queue deleted successfully');
        await dispatch('loadQueues', serverId);
      } catch (error) {
        logger.handleApiError('AzureStore', 'deleting queue', error);
        throw error;
      }
    },

    async sendMessage({ dispatch }, messageDetails) {
      try {
        await apiClient.post('/azure/servicebus/queue/message', messageDetails);
        logger.showSuccess('Message sent successfully');
      } catch (error) {
        logger.handleApiError('AzureStore', 'sending message', error);
        throw error;
      }
    },

    async receiveMessages({ commit }, { serverId, queueName }) {
      try {
        const response = await apiClient.get(`/azure/servicebus/queue/${serverId}/${queueName}/messages`);
        return response.data;
      } catch (error) {
        logger.handleApiError('AzureStore', 'receiving messages', error);
        throw error;
      }
    },

    // TOPIC ACTIONS
    async loadTopics({ commit }, serverId) {
      try {
        commit('setLoading', true);
        commit('clearError');
        const response = await apiClient.get(`/azure/servicebus/topics/${serverId}`);
        commit('setTopics', response.data);
      } catch (error) {
        commit('setError', 'Failed to load topics');
        logger.handleApiError('AzureStore', 'loading topics', error);
        throw error;
      } finally {
        commit('setLoading', false);
      }
    },

    async createTopic({ dispatch }, topicDetails) {
      try {
        await apiClient.post('/azure/servicebus/topic', topicDetails);
        logger.showSuccess('Topic created successfully');
        await dispatch('loadTopics', topicDetails.serverId);
      } catch (error) {
        logger.handleApiError('AzureStore', 'creating topic', error);
        throw error;
      }
    },

    async deleteTopic({ dispatch }, { serverId, topicName }) {
      try {
        await apiClient.delete('/azure/servicebus/topic', {
          data: { serverId, topicName },
        });
        logger.showSuccess('Topic deleted successfully');
        await dispatch('loadTopics', serverId);
      } catch (error) {
        logger.handleApiError('AzureStore', 'deleting topic', error);
        throw error;
      }
    },

    async publishMessage({ dispatch }, messageDetails) {
      try {
        await apiClient.post('/azure/servicebus/topic/message', messageDetails);
        logger.showSuccess('Message published successfully');
      } catch (error) {
        logger.handleApiError('AzureStore', 'publishing message', error);
        throw error;
      }
    },

    // SUBSCRIPTION ACTIONS
    async loadSubscriptions({ commit }, { serverId, topicName }) {
      try {
        const response = await apiClient.get(`/azure/servicebus/topic/${serverId}/${topicName}/subscriptions`);
        commit('setSubscriptions', response.data);
      } catch (error) {
        logger.handleApiError('AzureStore', 'loading subscriptions', error);
        throw error;
      }
    },

    async createSubscription({ dispatch }, subscriptionDetails) {
      try {
        await apiClient.post('/azure/servicebus/subscription', subscriptionDetails);
        logger.showSuccess('Subscription created successfully');
        await dispatch('loadSubscriptions', {
          serverId: subscriptionDetails.serverId,
          topicName: subscriptionDetails.topicName,
        });
      } catch (error) {
        logger.handleApiError('AzureStore', 'creating subscription', error);
        throw error;
      }
    },

    async deleteSubscription({ dispatch }, { serverId, topicName, subscriptionName }) {
      try {
        await apiClient.delete('/azure/servicebus/subscription', {
          data: { serverId, topicName, subscriptionName },
        });
        logger.showSuccess('Subscription deleted successfully');
        await dispatch('loadSubscriptions', { serverId, topicName });
      } catch (error) {
        logger.handleApiError('AzureStore', 'deleting subscription', error);
        throw error;
      }
    },
  },
};
