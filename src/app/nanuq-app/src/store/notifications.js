/**
 * Notifications Vuex Store Module
 * Manages application-wide toast notifications using Vuetify v-snackbar
 */

export default {
  namespaced: true,
  state: {
    snackbar: {
      show: false,
      message: '',
      color: 'info',
      timeout: 5000,
    },
  },
  mutations: {
    SHOW_NOTIFICATION(state, { message, color = 'info', timeout = 5000 }) {
      state.snackbar.show = true;
      state.snackbar.message = message;
      state.snackbar.color = color;
      state.snackbar.timeout = timeout;
    },
    HIDE_NOTIFICATION(state) {
      state.snackbar.show = false;
    },
  },
  actions: {
    showSuccess({ commit }, message) {
      commit('SHOW_NOTIFICATION', { message, color: 'success', timeout: 4000 });
    },
    showError({ commit }, message) {
      commit('SHOW_NOTIFICATION', { message, color: 'error', timeout: 6000 });
    },
    showWarning({ commit }, message) {
      commit('SHOW_NOTIFICATION', { message, color: 'warning', timeout: 5000 });
    },
    showInfo({ commit }, message) {
      commit('SHOW_NOTIFICATION', { message, color: 'info', timeout: 4000 });
    },
    hide({ commit }) {
      commit('HIDE_NOTIFICATION');
    },
  },
};
