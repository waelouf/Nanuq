/* eslint-disable no-unused-vars */
import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';

export default {
  namespaced: true,
  state: {
    credentialMetadata: {}, // { "kafka-1": { id, serverId, serverType, hasCredentials, createdAt, lastUsedAt } }
  },
  getters: {
    hasCredentials: (state) => (serverType, serverId) => {
      const key = `${serverType}-${serverId}`;
      return state.credentialMetadata[key]?.hasCredentials || false;
    },
    getCredentialId: (state) => (serverType, serverId) => {
      const key = `${serverType}-${serverId}`;
      return state.credentialMetadata[key]?.id || null;
    },
    getMetadata: (state) => (serverType, serverId) => {
      const key = `${serverType}-${serverId}`;
      return state.credentialMetadata[key] || null;
    },
  },
  mutations: {
    setCredentialMetadata(state, { serverType, serverId, metadata }) {
      const key = `${serverType}-${serverId}`;
      state.credentialMetadata[key] = {
        ...metadata,
        hasCredentials: true,
      };
    },
    removeCredentialMetadata(state, { serverType, serverId }) {
      const key = `${serverType}-${serverId}`;
      delete state.credentialMetadata[key];
    },
  },
  actions: {
    async fetchCredentialMetadata({ commit }, { serverType, serverId }) {
      try {
        const result = await apiClient.get(`/credentials/${serverId}/${serverType}`);
        commit('setCredentialMetadata', {
          serverType,
          serverId,
          metadata: result.data,
        });
        return result.data;
      } catch (error) {
        if (error.response?.status === 404) {
          // No credentials found - this is normal
          commit('removeCredentialMetadata', { serverType, serverId });
          return null;
        }
        logger.handleApiError('CredentialsStore', 'fetching credential metadata', error);
        throw error;
      }
    },
    async saveCredentials({ commit }, { serverId, serverType, username, password, additionalConfig }) {
      try {
        const result = await apiClient.post('/credentials', {
          serverId,
          serverType,
          username,
          password,
          additionalConfig,
        });
        // Fetch metadata after saving
        const metadata = await apiClient.get(`/credentials/${serverId}/${serverType}`);
        commit('setCredentialMetadata', {
          serverType,
          serverId,
          metadata: metadata.data,
        });
        return result.data; // Returns credential ID
      } catch (error) {
        logger.handleApiError('CredentialsStore', 'saving credentials', error);
        throw error;
      }
    },
    async testConnection({ commit }, { serverId, serverType, username, password }) {
      try {
        const result = await apiClient.post('/credentials/test', {
          serverId,
          serverType,
          username,
          password,
        });
        return result.data; // { success: bool, message: string }
      } catch (error) {
        logger.handleApiError('CredentialsStore', 'testing connection', error);
        throw error;
      }
    },
    async updateCredentials({ commit }, { credentialId, username, password, additionalConfig, serverId, serverType }) {
      try {
        const result = await apiClient.put(`/credentials/${credentialId}`, {
          username,
          password,
          additionalConfig,
        });
        // Fetch updated metadata
        const metadata = await apiClient.get(`/credentials/${serverId}/${serverType}`);
        commit('setCredentialMetadata', {
          serverType,
          serverId,
          metadata: metadata.data,
        });
        return result.data;
      } catch (error) {
        logger.handleApiError('CredentialsStore', 'updating credentials', error);
        throw error;
      }
    },
    async deleteCredentials({ commit }, { credentialId, serverType, serverId }) {
      try {
        const result = await apiClient.delete(`/credentials/${credentialId}`);
        commit('removeCredentialMetadata', { serverType, serverId });
        return result.data;
      } catch (error) {
        logger.handleApiError('CredentialsStore', 'deleting credentials', error);
        throw error;
      }
    },
  },
};
