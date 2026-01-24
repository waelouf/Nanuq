import { describe, it, expect, beforeEach, vi } from 'vitest';
import { createStore } from 'vuex';
import notificationsModule from '../notifications';

describe('Notifications Store', () => {
  let store;

  beforeEach(() => {
    store = createStore({
      modules: {
        notifications: notificationsModule,
      },
    });
    vi.clearAllMocks();
  });

  // ==================== State Tests ====================
  describe('State', () => {
    it('should initialize with correct default state', () => {
      const state = store.state.notifications.snackbar;

      expect(state.show).toBe(false);
      expect(state.message).toBe('');
      expect(state.color).toBe('info');
      expect(state.timeout).toBe(5000);
    });
  });

  // ==================== Mutations Tests ====================
  describe('Mutations', () => {
    it('should show notification with custom message and color', () => {
      store.commit('notifications/SHOW_NOTIFICATION', {
        message: 'Test message',
        color: 'success',
        timeout: 3000,
      });

      const snackbar = store.state.notifications.snackbar;
      expect(snackbar.show).toBe(true);
      expect(snackbar.message).toBe('Test message');
      expect(snackbar.color).toBe('success');
      expect(snackbar.timeout).toBe(3000);
    });

    it('should use default color and timeout when not provided', () => {
      store.commit('notifications/SHOW_NOTIFICATION', {
        message: 'Test message',
      });

      const snackbar = store.state.notifications.snackbar;
      expect(snackbar.show).toBe(true);
      expect(snackbar.message).toBe('Test message');
      expect(snackbar.color).toBe('info');
      expect(snackbar.timeout).toBe(5000);
    });

    it('should hide notification', () => {
      store.commit('notifications/SHOW_NOTIFICATION', {
        message: 'Test message',
        color: 'success',
      });
      expect(store.state.notifications.snackbar.show).toBe(true);

      store.commit('notifications/HIDE_NOTIFICATION');
      expect(store.state.notifications.snackbar.show).toBe(false);
    });
  });

  // ==================== Actions Tests ====================
  describe('Actions', () => {
    it('should show success notification with correct defaults', () => {
      store.dispatch('notifications/showSuccess', 'Operation successful');

      const snackbar = store.state.notifications.snackbar;
      expect(snackbar.show).toBe(true);
      expect(snackbar.message).toBe('Operation successful');
      expect(snackbar.color).toBe('success');
      expect(snackbar.timeout).toBe(4000);
    });

    it('should show error notification with correct defaults', () => {
      store.dispatch('notifications/showError', 'Operation failed');

      const snackbar = store.state.notifications.snackbar;
      expect(snackbar.show).toBe(true);
      expect(snackbar.message).toBe('Operation failed');
      expect(snackbar.color).toBe('error');
      expect(snackbar.timeout).toBe(6000);
    });

    it('should show warning notification with correct defaults', () => {
      store.dispatch('notifications/showWarning', 'Warning message');

      const snackbar = store.state.notifications.snackbar;
      expect(snackbar.show).toBe(true);
      expect(snackbar.message).toBe('Warning message');
      expect(snackbar.color).toBe('warning');
      expect(snackbar.timeout).toBe(5000);
    });

    it('should show info notification with correct defaults', () => {
      store.dispatch('notifications/showInfo', 'Info message');

      const snackbar = store.state.notifications.snackbar;
      expect(snackbar.show).toBe(true);
      expect(snackbar.message).toBe('Info message');
      expect(snackbar.color).toBe('info');
      expect(snackbar.timeout).toBe(4000);
    });

    it('should hide notification', () => {
      store.dispatch('notifications/showSuccess', 'Test');
      expect(store.state.notifications.snackbar.show).toBe(true);

      store.dispatch('notifications/hide');
      expect(store.state.notifications.snackbar.show).toBe(false);
    });

    it('should verify all actions have different timeout values', () => {
      const timeouts = {};

      store.dispatch('notifications/showSuccess', 'Test');
      timeouts.success = store.state.notifications.snackbar.timeout;

      store.dispatch('notifications/showError', 'Test');
      timeouts.error = store.state.notifications.snackbar.timeout;

      store.dispatch('notifications/showWarning', 'Test');
      timeouts.warning = store.state.notifications.snackbar.timeout;

      store.dispatch('notifications/showInfo', 'Test');
      timeouts.info = store.state.notifications.snackbar.timeout;

      expect(timeouts.success).toBe(4000);
      expect(timeouts.error).toBe(6000); // Longer timeout for errors
      expect(timeouts.warning).toBe(5000);
      expect(timeouts.info).toBe(4000);
    });

    it('should replace previous notification when showing new one', () => {
      store.dispatch('notifications/showSuccess', 'First message');
      expect(store.state.notifications.snackbar.message).toBe('First message');
      expect(store.state.notifications.snackbar.color).toBe('success');

      store.dispatch('notifications/showError', 'Second message');
      expect(store.state.notifications.snackbar.message).toBe('Second message');
      expect(store.state.notifications.snackbar.color).toBe('error');
    });
  });
});
