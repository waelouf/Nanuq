import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';

export default {
  namespaced: true,
  state: {
    activityLogs: [],
    activityTypes: [],
    loading: false,
    error: null,
  },
  getters: {
    logsWithTypeData: (state) => {
      return state.activityLogs.map((log) => ({
        ...log,
        type: state.activityTypes.find((t) => t.id === log.activityTypeId),
      }));
    },
  },
  mutations: {
    setActivityLogs(state, logs) {
      state.activityLogs = logs;
    },
    setActivityTypes(state, types) {
      state.activityTypes = types;
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
    async loadActivityLogs({ commit }) {
      try {
        commit('setLoading', true);
        commit('clearError');
        const response = await apiClient.get('/activitylog');
        commit('setActivityLogs', response.data);
      } catch (error) {
        commit('setError', 'Failed to load activity logs');
        logger.handleApiError('ActivityLogStore', 'loading activity logs', error);
        throw error;
      } finally {
        commit('setLoading', false);
      }
    },
    async loadActivityTypes({ commit }) {
      try {
        const response = await apiClient.get('/activitylog/types');
        commit('setActivityTypes', response.data);
      } catch (error) {
        logger.handleApiError('ActivityLogStore', 'loading activity types', error);
        throw error;
      }
    },
    async refreshLogs({ dispatch }) {
      await Promise.all([
        dispatch('loadActivityLogs'),
        dispatch('loadActivityTypes'),
      ]);
    },
  },
};
