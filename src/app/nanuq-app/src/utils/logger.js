/* eslint-disable no-console */
/**
 * Logger utility for consistent logging throughout the application
 * Automatically disables logs in production mode
 */

// Only enable logging in development environment
const isDevMode = process.env.NODE_ENV !== 'production';

const logger = {
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
   * Handle API errors with consistent logging and optional callback
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

    // Execute callback if provided
    if (callback && typeof callback === 'function') {
      callback(error);
    }
  },
};

export default logger;
