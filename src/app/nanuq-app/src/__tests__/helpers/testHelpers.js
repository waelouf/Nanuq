/**
 * Test Helpers for Nanuq Frontend Tests
 * Provides reusable utilities for Vuex store and component testing
 */

import { createStore } from 'vuex';
import { createRouter, createMemoryHistory } from 'vue-router';
import { vi } from 'vitest';

/**
 * Creates a mock Vuex store with specified modules
 * @param {Object} modules - Vuex modules to include
 * @returns {Store} Configured Vuex store instance
 */
export function createMockStore(modules = {}) {
  return createStore({
    modules,
  });
}

/**
 * Creates a mock Vue Router instance
 * @param {Array} routes - Routes to configure (optional)
 * @returns {Router} Configured router instance
 */
export function createMockRouter(routes = []) {
  return createRouter({
    history: createMemoryHistory(),
    routes,
  });
}

/**
 * Creates a mock apiClient with common methods
 * @returns {Object} Mock apiClient object
 */
export function mockApiClient() {
  return {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
    patch: vi.fn(),
  };
}

/**
 * Creates a mock logger
 * @returns {Object} Mock logger object
 */
export function mockLogger() {
  return {
    success: vi.fn(),
    showSuccess: vi.fn(),
    error: vi.fn(),
    warn: vi.fn(),
    info: vi.fn(),
    handleApiError: vi.fn(),
  };
}

/**
 * Creates a mock error response from API
 * @param {string} message - Error message
 * @param {number} status - HTTP status code
 * @param {Object} data - Optional response data
 * @returns {Object} Mock error object
 */
export function createMockError(message, status = 500, data = null) {
  const error = new Error(message);
  error.response = {
    status,
    data: data || message,
  };
  return error;
}

/**
 * Creates a mock AWS auth error
 * @param {string} type - Type of auth error ('token', 'credentials', 'unauthorized')
 * @returns {Object} Mock AWS auth error
 */
export function createAwsAuthError(type = 'token') {
  const messages = {
    token: 'The security token included in the request is invalid',
    credentials: 'Invalid credentials provided',
    unauthorized: 'Unauthorized access',
  };

  return createMockError(messages[type] || messages.token, 403, {
    message: messages[type] || messages.token,
  });
}

/**
 * Helper to wait for next tick in async tests
 * @returns {Promise} Resolves on next tick
 */
export function nextTick() {
  return new Promise((resolve) => setTimeout(resolve, 0));
}

/**
 * Helper to flush all pending promises
 * @returns {Promise} Resolves when all promises are flushed
 */
export async function flushPromises() {
  return new Promise((resolve) => setImmediate(resolve));
}
