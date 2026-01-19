import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';

// Helper function to check if error is AWS auth error
function isAwsAuthError(error) {
  // Get error message from various possible locations
  let errorMessage = '';
  if (typeof error.response?.data === 'string') {
    errorMessage = error.response.data;
  } else if (error.response?.data?.message) {
    errorMessage = error.response.data.message;
  } else if (error.message) {
    errorMessage = error.message;
  }

  const lowerMessage = errorMessage.toLowerCase();
  return lowerMessage.includes('security token') ||
         lowerMessage.includes('invalid') && lowerMessage.includes('credentials') ||
         lowerMessage.includes('unauthorized');
}

export default {
  namespaced: true,
  state: {
    // SQS state
    sqsQueues: {},        // { serverId: [queues] }
    sqsQueueDetails: {},  // { 'serverId-queueUrl': details }
    sqsMessages: {},      // { 'serverId-queueUrl': [messages] }

    // SNS state
    snsTopics: {},        // { serverId: [topics] }
    snsTopicDetails: {},  // { 'serverId-topicArn': details }
    snsSubscriptions: {}, // { 'serverId-topicArn': [subscriptions] }

    loading: false,
    error: null,
  },
  getters: {
    getSqsQueues: (state) => (serverId) => state.sqsQueues[serverId] || [],
    getSqsQueueDetails: (state) => (serverId, queueUrl) => {
      const key = `${serverId}-${encodeURIComponent(queueUrl)}`;
      return state.sqsQueueDetails[key];
    },
    getSqsMessages: (state) => (serverId, queueUrl) => {
      const key = `${serverId}-${encodeURIComponent(queueUrl)}`;
      return state.sqsMessages[key] || [];
    },

    getSnsTopics: (state) => (serverId) => state.snsTopics[serverId] || [],
    getSnsTopicDetails: (state) => (serverId, topicArn) => {
      const key = `${serverId}-${encodeURIComponent(topicArn)}`;
      return state.snsTopicDetails[key];
    },
    getSnsSubscriptions: (state) => (serverId, topicArn) => {
      const key = `${serverId}-${encodeURIComponent(topicArn)}`;
      return state.snsSubscriptions[key] || [];
    },
  },
  mutations: {
    // SQS mutations
    updateSqsQueues(state, { serverId, data }) {
      state.sqsQueues[serverId] = data;
    },
    updateSqsQueueDetails(state, { serverId, queueUrl, data }) {
      const key = `${serverId}-${encodeURIComponent(queueUrl)}`;
      state.sqsQueueDetails[key] = data;
    },
    updateSqsMessages(state, { serverId, queueUrl, data }) {
      const key = `${serverId}-${encodeURIComponent(queueUrl)}`;
      state.sqsMessages[key] = data;
    },

    // SNS mutations
    updateSnsTopics(state, { serverId, data }) {
      state.snsTopics[serverId] = data;
    },
    updateSnsTopicDetails(state, { serverId, topicArn, data }) {
      const key = `${serverId}-${encodeURIComponent(topicArn)}`;
      state.snsTopicDetails[key] = data;
    },
    updateSnsSubscriptions(state, { serverId, topicArn, data }) {
      const key = `${serverId}-${encodeURIComponent(topicArn)}`;
      state.snsSubscriptions[key] = data;
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
    // SQS actions
    async loadSqsQueues({ commit }, serverId) {
      try {
        commit('setLoading', true);
        commit('clearError');
        const response = await apiClient.get(`/aws/sqs/queues/${serverId}`);
        commit('updateSqsQueues', { serverId, data: response.data });
      } catch (error) {
        commit('setError', 'Failed to load SQS queues');
        logger.handleApiError('AWSStore', 'loading SQS queues', error);
        // Check if this is an authentication error
        if (isAwsAuthError(error)) {
          const authError = new Error('AWS_AUTH_ERROR');
          authError.originalError = error;
          throw authError;
        }
        throw error;
      } finally {
        commit('setLoading', false);
      }
    },

    async loadSqsQueueDetails({ commit }, { serverId, queueUrl }) {
      try {
        const response = await apiClient.get(
          `/aws/sqs/queue/details/${serverId}?queueUrl=${encodeURIComponent(queueUrl)}`,
        );
        commit('updateSqsQueueDetails', { serverId, queueUrl, data: response.data });
      } catch (error) {
        logger.handleApiError('AWSStore', 'loading queue details', error);
        // Check if this is an authentication error
        if (isAwsAuthError(error)) {
          const authError = new Error('AWS_AUTH_ERROR');
          authError.originalError = error;
          throw authError;
        }
        throw error;
      }
    },

    async createQueue({ dispatch }, queueDetails) {
      try {
        await apiClient.post('/aws/sqs/queue/create', queueDetails);
        await dispatch('loadSqsQueues', queueDetails.serverId);
        logger.success('SQS queue created successfully');
      } catch (error) {
        logger.handleApiError('AWSStore', 'creating queue', error);
        throw error;
      }
    },

    async deleteQueue({ dispatch }, { serverId, queueUrl }) {
      try {
        await apiClient.delete(
          `/aws/sqs/queue/${serverId}?queueUrl=${encodeURIComponent(queueUrl)}`,
        );
        await dispatch('loadSqsQueues', serverId);
        logger.success('SQS queue deleted successfully');
      } catch (error) {
        logger.handleApiError('AWSStore', 'deleting queue', error);
        throw error;
      }
    },

    async sendMessage({ commit }, messageDetails) {
      try {
        await apiClient.post('/aws/sqs/message/send', messageDetails);
        logger.success('Message sent successfully');
      } catch (error) {
        logger.handleApiError('AWSStore', 'sending message', error);
        throw error;
      }
    },

    async receiveMessages({ commit }, { serverId, queueUrl, ...receiveDetails }) {
      try {
        const response = await apiClient.post('/aws/sqs/message/receive', {
          serverId,
          queueUrl,
          ...receiveDetails,
        });
        commit('updateSqsMessages', { serverId, queueUrl, data: response.data });
        return response.data;
      } catch (error) {
        logger.handleApiError('AWSStore', 'receiving messages', error);
        throw error;
      }
    },

    async deleteMessage({ commit }, { serverId, queueUrl, receiptHandle }) {
      try {
        await apiClient.delete(
          `/aws/sqs/message/${serverId}?queueUrl=${encodeURIComponent(queueUrl)}&receiptHandle=${encodeURIComponent(receiptHandle)}`,
        );
        logger.success('Message deleted successfully');
      } catch (error) {
        logger.handleApiError('AWSStore', 'deleting message', error);
        throw error;
      }
    },

    // SNS actions
    async loadSnsTopics({ commit }, serverId) {
      try {
        commit('setLoading', true);
        commit('clearError');
        const response = await apiClient.get(`/aws/sns/topics/${serverId}`);
        commit('updateSnsTopics', { serverId, data: response.data });
      } catch (error) {
        commit('setError', 'Failed to load SNS topics');
        logger.handleApiError('AWSStore', 'loading SNS topics', error);
        // Check if this is an authentication error
        if (isAwsAuthError(error)) {
          const authError = new Error('AWS_AUTH_ERROR');
          authError.originalError = error;
          throw authError;
        }
        throw error;
      } finally {
        commit('setLoading', false);
      }
    },

    async loadSnsTopicDetails({ commit }, { serverId, topicArn }) {
      try {
        const response = await apiClient.get(
          `/aws/sns/topic/details/${serverId}?topicArn=${encodeURIComponent(topicArn)}`,
        );
        commit('updateSnsTopicDetails', { serverId, topicArn, data: response.data });
      } catch (error) {
        logger.handleApiError('AWSStore', 'loading topic details', error);
        // Check if this is an authentication error
        if (isAwsAuthError(error)) {
          const authError = new Error('AWS_AUTH_ERROR');
          authError.originalError = error;
          throw authError;
        }
        throw error;
      }
    },

    async createTopic({ dispatch }, topicDetails) {
      try {
        await apiClient.post('/aws/sns/topic/create', topicDetails);
        await dispatch('loadSnsTopics', topicDetails.serverId);
        logger.success('SNS topic created successfully');
      } catch (error) {
        logger.handleApiError('AWSStore', 'creating topic', error);
        throw error;
      }
    },

    async deleteTopic({ dispatch }, { serverId, topicArn }) {
      try {
        await apiClient.delete(
          `/aws/sns/topic/${serverId}?topicArn=${encodeURIComponent(topicArn)}`,
        );
        await dispatch('loadSnsTopics', serverId);
        logger.success('SNS topic deleted successfully');
      } catch (error) {
        logger.handleApiError('AWSStore', 'deleting topic', error);
        throw error;
      }
    },

    async publishMessage({ commit }, messageDetails) {
      try {
        await apiClient.post('/aws/sns/message/publish', messageDetails);
        logger.success('Message published successfully');
      } catch (error) {
        logger.handleApiError('AWSStore', 'publishing message', error);
        throw error;
      }
    },

    async loadSubscriptions({ commit }, { serverId, topicArn }) {
      try {
        const response = await apiClient.get(
          `/aws/sns/subscriptions/${serverId}?topicArn=${encodeURIComponent(topicArn)}`,
        );
        commit('updateSnsSubscriptions', { serverId, topicArn, data: response.data });
      } catch (error) {
        logger.handleApiError('AWSStore', 'loading subscriptions', error);
        // Check if this is an authentication error
        if (isAwsAuthError(error)) {
          const authError = new Error('AWS_AUTH_ERROR');
          authError.originalError = error;
          throw authError;
        }
        throw error;
      }
    },

    async subscribe({ dispatch }, subscriptionDetails) {
      try {
        await apiClient.post('/aws/sns/subscription/subscribe', subscriptionDetails);
        await dispatch('loadSubscriptions', {
          serverId: subscriptionDetails.serverId,
          topicArn: subscriptionDetails.topicArn,
        });
        logger.success('Subscribed successfully');
      } catch (error) {
        logger.handleApiError('AWSStore', 'subscribing', error);
        throw error;
      }
    },

    async unsubscribe({ dispatch }, { serverId, topicArn, subscriptionArn }) {
      try {
        await apiClient.delete(
          `/aws/sns/subscription/${serverId}?subscriptionArn=${encodeURIComponent(subscriptionArn)}`,
        );
        await dispatch('loadSubscriptions', { serverId, topicArn });
        logger.success('Unsubscribed successfully');
      } catch (error) {
        logger.handleApiError('AWSStore', 'unsubscribing', error);
        throw error;
      }
    },
  },
};
