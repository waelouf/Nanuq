/* eslint-disable no-console */
/**
 * Logger utility for consistent logging throughout the application
 * Automatically disables logs in production mode
 * Dispatches user-friendly notifications to the UI
 */

// Only enable logging in development environment
const isDevMode = process.env.NODE_ENV !== 'production';

// Store instance will be injected
let store = null;

const logger = {
  /**
   * Initialize logger with Vuex store for notifications
   * @param {Object} storeInstance - Vuex store instance
   */
  init(storeInstance) {
    store = storeInstance;
  },
  /**
   * Log information messages
   * @param {string} component - Component name for context
   * @param {string} message - Message to log
   * @param {any} data - Optional data to log
   */
  info(component, message, data) {
    if (isDevMode) {
      if (data) {
        console.info(`[${component}] ${message}`, data);
      } else {
        console.info(`[${component}] ${message}`);
      }
    }
  },

  /**
   * Log warning messages
   * @param {string} component - Component name for context
   * @param {string} message - Message to log
   * @param {any} data - Optional data to log
   */
  warn(component, message, data) {
    if (isDevMode) {
      if (data) {
        console.warn(`[${component}] ${message}`, data);
      } else {
        console.warn(`[${component}] ${message}`);
      }
    }
  },

  /**
   * Log error messages
   * @param {string} component - Component name for context
   * @param {string} message - Message to log
   * @param {Error|any} error - Error object or data
   */
  error(component, message, error) {
    if (isDevMode) {
      if (error) {
        console.error(`[${component}] ${message}`, error);
      } else {
        console.error(`[${component}] ${message}`);
      }
    }
  },

  /**
   * Handle API errors with consistent logging and user notifications
   * @param {string} component - Component name for context
   * @param {string} operation - Description of the operation that failed
   * @param {Error} error - The error object from the catch block
   * @param {Function} [callback] - Optional callback to handle the error
   */
  handleApiError(component, operation, error, callback = null) {
    if (isDevMode) {
      console.error(`[${component}] API Error during ${operation}:`, error);
    }

    // If status is available, log it for debugging
    const status = error.response?.status;
    if (status && isDevMode) {
      console.error(`[${component}] API Error Status: ${status}`);
    }

    // Show user-friendly error notification
    if (store) {
      const errorMessage = error.response?.data?.message
        || error.message
        || `Failed to ${operation}`;

      store.dispatch('notifications/showError', errorMessage);
    }

    // Execute callback if provided
    if (callback && typeof callback === 'function') {
      callback(error);
    }
  },

  /**
   * Show success notification to user
   * @param {string} message - Success message
   */
  success(message) {
    if (store) {
      store.dispatch('notifications/showSuccess', message);
    }
  },

  /**
   * Show warning notification to user
   * @param {string} message - Warning message
   */
  warning(message) {
    if (store) {
      store.dispatch('notifications/showWarning', message);
    }
  },
};

export default logger;
