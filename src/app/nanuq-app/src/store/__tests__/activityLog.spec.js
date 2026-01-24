import { describe, it, expect, beforeEach, vi } from 'vitest';
import { createStore } from 'vuex';
import activityLogModule from '../activityLog';
import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';
import { createMockError } from '@/__tests__/helpers/testHelpers';
import {
  mockActivityLogs,
  mockActivityTypes,
  mockActivityLogsWithTypes,
} from '@/__tests__/fixtures/mockData';

vi.mock('@/services/apiClient');
vi.mock('@/utils/logger');

describe('Activity Log Store', () => {
  let store;

  beforeEach(() => {
    store = createStore({
      modules: {
        activityLog: activityLogModule,
      },
    });
    vi.clearAllMocks();
  });

  // ==================== State Tests ====================
  describe('State', () => {
    it('should initialize with correct default state', () => {
      const state = store.state.activityLog;

      expect(state.activityLogs).toEqual([]);
      expect(state.activityTypes).toEqual([]);
      expect(state.loading).toBe(false);
      expect(state.error).toBe(null);
    });
  });

  // ==================== Getters Tests ====================
  describe('Getters', () => {
    it('should join activity logs with type data', () => {
      store.state.activityLog.activityLogs = mockActivityLogs;
      store.state.activityLog.activityTypes = mockActivityTypes;

      const logsWithTypes = store.getters['activityLog/logsWithTypeData'];

      expect(logsWithTypes).toHaveLength(3);
      expect(logsWithTypes[0]).toEqual({
        ...mockActivityLogs[0],
        type: mockActivityTypes[0],
      });
      expect(logsWithTypes[1]).toEqual({
        ...mockActivityLogs[1],
        type: mockActivityTypes[3],
      });
    });

    it('should return logs with undefined type when type not found', () => {
      store.state.activityLog.activityLogs = [
        {
          id: 99,
          activityTypeId: 999,
          details: 'Unknown type',
        },
      ];
      store.state.activityLog.activityTypes = mockActivityTypes;

      const logsWithTypes = store.getters['activityLog/logsWithTypeData'];

      expect(logsWithTypes[0].type).toBeUndefined();
    });

    it('should return empty array when no logs exist', () => {
      store.state.activityLog.activityLogs = [];
      store.state.activityLog.activityTypes = mockActivityTypes;

      const logsWithTypes = store.getters['activityLog/logsWithTypeData'];

      expect(logsWithTypes).toEqual([]);
    });
  });

  // ==================== Mutations Tests ====================
  describe('Mutations', () => {
    it('should set activity logs', () => {
      store.commit('activityLog/setActivityLogs', mockActivityLogs);
      expect(store.state.activityLog.activityLogs).toEqual(mockActivityLogs);
    });

    it('should set activity types', () => {
      store.commit('activityLog/setActivityTypes', mockActivityTypes);
      expect(store.state.activityLog.activityTypes).toEqual(mockActivityTypes);
    });

    it('should set loading state', () => {
      store.commit('activityLog/setLoading', true);
      expect(store.state.activityLog.loading).toBe(true);

      store.commit('activityLog/setLoading', false);
      expect(store.state.activityLog.loading).toBe(false);
    });

    it('should set error message', () => {
      const errorMsg = 'Failed to load activity logs';
      store.commit('activityLog/setError', errorMsg);
      expect(store.state.activityLog.error).toBe(errorMsg);
    });

    it('should clear error', () => {
      store.commit('activityLog/setError', 'Some error');
      store.commit('activityLog/clearError');
      expect(store.state.activityLog.error).toBe(null);
    });
  });

  // ==================== Actions Tests ====================
  describe('Actions', () => {
    describe('loadActivityLogs', () => {
      it('should load activity logs successfully', async () => {
        apiClient.get.mockResolvedValue({ data: mockActivityLogs });

        await store.dispatch('activityLog/loadActivityLogs');

        expect(apiClient.get).toHaveBeenCalledWith('/activitylog');
        expect(store.state.activityLog.activityLogs).toEqual(mockActivityLogs);
        expect(store.state.activityLog.loading).toBe(false);
        expect(store.state.activityLog.error).toBe(null);
      });

      it('should set loading state during loadActivityLogs', async () => {
        let loadingDuringCall = false;

        apiClient.get.mockImplementation(() => {
          loadingDuringCall = store.state.activityLog.loading;
          return Promise.resolve({ data: mockActivityLogs });
        });

        await store.dispatch('activityLog/loadActivityLogs');

        expect(loadingDuringCall).toBe(true);
        expect(store.state.activityLog.loading).toBe(false);
      });

      it('should handle error when loading activity logs', async () => {
        const error = createMockError('Network error');
        apiClient.get.mockRejectedValue(error);

        await expect(store.dispatch('activityLog/loadActivityLogs')).rejects.toThrow();
        expect(store.state.activityLog.error).toBe('Failed to load activity logs');
        expect(store.state.activityLog.loading).toBe(false);
        expect(logger.handleApiError).toHaveBeenCalledWith(
          'ActivityLogStore',
          'loading activity logs',
          error
        );
      });
    });

    describe('loadActivityTypes', () => {
      it('should load activity types successfully', async () => {
        apiClient.get.mockResolvedValue({ data: mockActivityTypes });

        await store.dispatch('activityLog/loadActivityTypes');

        expect(apiClient.get).toHaveBeenCalledWith('/activitylog/types');
        expect(store.state.activityLog.activityTypes).toEqual(mockActivityTypes);
      });

      it('should handle error when loading activity types', async () => {
        const error = createMockError('Network error');
        apiClient.get.mockRejectedValue(error);

        await expect(store.dispatch('activityLog/loadActivityTypes')).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith(
          'ActivityLogStore',
          'loading activity types',
          error
        );
      });
    });

    describe('refreshLogs', () => {
      it('should dispatch both loadActivityLogs and loadActivityTypes in parallel', async () => {
        apiClient.get.mockImplementation((url) => {
          if (url === '/activitylog') {
            return Promise.resolve({ data: mockActivityLogs });
          }
          if (url === '/activitylog/types') {
            return Promise.resolve({ data: mockActivityTypes });
          }
          return Promise.reject(new Error('Unknown URL'));
        });

        await store.dispatch('activityLog/refreshLogs');

        expect(apiClient.get).toHaveBeenCalledWith('/activitylog');
        expect(apiClient.get).toHaveBeenCalledWith('/activitylog/types');
        expect(store.state.activityLog.activityLogs).toEqual(mockActivityLogs);
        expect(store.state.activityLog.activityTypes).toEqual(mockActivityTypes);
      });

      it('should handle error when refreshLogs fails', async () => {
        const error = createMockError('Network error');
        apiClient.get.mockRejectedValue(error);

        await expect(store.dispatch('activityLog/refreshLogs')).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalled();
      });

      it('should wait for both promises to complete', async () => {
        let logsResolved = false;
        let typesResolved = false;

        apiClient.get.mockImplementation((url) => {
          if (url === '/activitylog') {
            return new Promise((resolve) => {
              setTimeout(() => {
                logsResolved = true;
                resolve({ data: mockActivityLogs });
              }, 10);
            });
          }
          if (url === '/activitylog/types') {
            return new Promise((resolve) => {
              setTimeout(() => {
                typesResolved = true;
                resolve({ data: mockActivityTypes });
              }, 10);
            });
          }
          return Promise.reject(new Error('Unknown URL'));
        });

        await store.dispatch('activityLog/refreshLogs');

        expect(logsResolved).toBe(true);
        expect(typesResolved).toBe(true);
      });
    });
  });
});
