import { describe, it, expect, beforeEach, vi } from 'vitest';
import { createStore } from 'vuex';
import credentialsModule from '../credentials';
import apiClient from '@/services/apiClient';
import logger from '@/utils/logger';
import { createMockError } from '@/__tests__/helpers/testHelpers';
import { mockCredentialMetadata, mockCredentialsMap } from '@/__tests__/fixtures/mockData';

vi.mock('@/services/apiClient');
vi.mock('@/utils/logger');

describe('Credentials Store', () => {
  let store;

  beforeEach(() => {
    store = createStore({
      modules: {
        credentials: credentialsModule,
      },
    });
    vi.clearAllMocks();
  });

  // ==================== State Tests ====================
  describe('State', () => {
    it('should initialize with empty credentialMetadata', () => {
      expect(store.state.credentials.credentialMetadata).toEqual({});
    });
  });

  // ==================== Getters Tests ====================
  describe('Getters', () => {
    beforeEach(() => {
      // Set up test data
      store.state.credentials.credentialMetadata = mockCredentialsMap;
    });

    it('should check if credentials exist for a server', () => {
      const hasCredentials = store.getters['credentials/hasCredentials']('AWS', 4);
      expect(hasCredentials).toBe(true);
    });

    it('should return false when credentials do not exist', () => {
      const hasCredentials = store.getters['credentials/hasCredentials']('Kafka', 999);
      expect(hasCredentials).toBe(false);
    });

    it('should get credential ID for a server', () => {
      const credentialId = store.getters['credentials/getCredentialId']('AWS', 4);
      expect(credentialId).toBe(1);
    });

    it('should return null when credential ID does not exist', () => {
      const credentialId = store.getters['credentials/getCredentialId']('Kafka', 999);
      expect(credentialId).toBe(null);
    });

    it('should get full metadata for a server', () => {
      const metadata = store.getters['credentials/getMetadata']('AWS', 4);
      expect(metadata).toEqual(mockCredentialsMap['AWS-4']);
    });

    it('should return null when metadata does not exist', () => {
      const metadata = store.getters['credentials/getMetadata']('Kafka', 999);
      expect(metadata).toBe(null);
    });

    it('should handle different server types', () => {
      const awsHasCredentials = store.getters['credentials/hasCredentials']('AWS', 4);
      const azureHasCredentials = store.getters['credentials/hasCredentials']('Azure', 5);

      expect(awsHasCredentials).toBe(true);
      expect(azureHasCredentials).toBe(true);
    });
  });

  // ==================== Mutations Tests ====================
  describe('Mutations', () => {
    it('should set credential metadata', () => {
      const serverType = 'Kafka';
      const serverId = 10;
      const metadata = {
        id: 5,
        serverId: 10,
        serverType: 'Kafka',
        username: 'kafka-user',
      };

      store.commit('credentials/setCredentialMetadata', { serverType, serverId, metadata });

      const key = `${serverType}-${serverId}`;
      expect(store.state.credentials.credentialMetadata[key]).toEqual({
        ...metadata,
        hasCredentials: true,
      });
    });

    it('should set hasCredentials to true automatically', () => {
      const serverType = 'Redis';
      const serverId = 2;
      const metadata = { id: 3, username: 'redis-user' };

      store.commit('credentials/setCredentialMetadata', { serverType, serverId, metadata });

      const key = `${serverType}-${serverId}`;
      expect(store.state.credentials.credentialMetadata[key].hasCredentials).toBe(true);
    });

    it('should remove credential metadata', () => {
      const serverType = 'AWS';
      const serverId = 4;
      store.state.credentials.credentialMetadata = mockCredentialsMap;

      store.commit('credentials/removeCredentialMetadata', { serverType, serverId });

      const key = `${serverType}-${serverId}`;
      expect(store.state.credentials.credentialMetadata[key]).toBeUndefined();
    });

    it('should handle removing non-existent metadata gracefully', () => {
      store.commit('credentials/removeCredentialMetadata', { serverType: 'Kafka', serverId: 999 });
      // Should not throw error
      expect(store.state.credentials.credentialMetadata['Kafka-999']).toBeUndefined();
    });
  });

  // ==================== Actions Tests ====================
  describe('Actions', () => {
    describe('fetchCredentialMetadata', () => {
      it('should fetch credential metadata successfully', async () => {
        const serverType = 'AWS';
        const serverId = 4;
        apiClient.get.mockResolvedValue({ data: mockCredentialMetadata });

        const result = await store.dispatch('credentials/fetchCredentialMetadata', { serverType, serverId });

        expect(apiClient.get).toHaveBeenCalledWith(`/credentials/${serverId}/${serverType}`);
        expect(result).toEqual(mockCredentialMetadata);
        const key = `${serverType}-${serverId}`;
        expect(store.state.credentials.credentialMetadata[key].hasCredentials).toBe(true);
      });

      it('should handle 404 error by returning null', async () => {
        const serverType = 'Kafka';
        const serverId = 1;
        const error = createMockError('Not found', 404);
        apiClient.get.mockRejectedValue(error);

        const result = await store.dispatch('credentials/fetchCredentialMetadata', { serverType, serverId });

        expect(result).toBe(null);
        const key = `${serverType}-${serverId}`;
        expect(store.state.credentials.credentialMetadata[key]).toBeUndefined();
      });

      it('should throw error for non-404 errors', async () => {
        const serverType = 'AWS';
        const serverId = 4;
        const error = createMockError('Internal server error', 500);
        apiClient.get.mockRejectedValue(error);

        await expect(
          store.dispatch('credentials/fetchCredentialMetadata', { serverType, serverId })
        ).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith(
          'CredentialsStore',
          'fetching credential metadata',
          error
        );
      });
    });

    describe('saveCredentials', () => {
      it('should save credentials successfully', async () => {
        const credentialData = {
          serverId: 1,
          serverType: 'Kafka',
          username: 'kafka-user',
          password: 'kafka-pass',
        };
        apiClient.post.mockResolvedValue({ data: { id: 10 } });
        apiClient.get.mockResolvedValue({ data: mockCredentialMetadata });

        const result = await store.dispatch('credentials/saveCredentials', credentialData);

        expect(apiClient.post).toHaveBeenCalledWith('/credentials', {
          serverId: 1,
          serverType: 'Kafka',
          username: 'kafka-user',
          password: 'kafka-pass',
          additionalConfig: undefined,
        });
        expect(apiClient.get).toHaveBeenCalledWith(`/credentials/${credentialData.serverId}/${credentialData.serverType}`);
        expect(logger.success).toHaveBeenCalledWith('Credentials saved successfully');
        expect(result).toEqual({ id: 10 });
      });

      it('should convert AWS sessionToken to additionalConfig JSON', async () => {
        const credentialData = {
          serverId: 4,
          serverType: 'AWS',
          username: 'AKIAIOSFODNN7EXAMPLE',
          password: 'secret-key',
          sessionToken: 'session-token-123',
        };
        apiClient.post.mockResolvedValue({ data: { id: 11 } });
        apiClient.get.mockResolvedValue({ data: mockCredentialMetadata });

        await store.dispatch('credentials/saveCredentials', credentialData);

        expect(apiClient.post).toHaveBeenCalledWith('/credentials', {
          serverId: 4,
          serverType: 'AWS',
          username: 'AKIAIOSFODNN7EXAMPLE',
          password: 'secret-key',
          additionalConfig: JSON.stringify({ SessionToken: 'session-token-123' }),
        });
      });

      it('should use provided additionalConfig when no sessionToken', async () => {
        const credentialData = {
          serverId: 5,
          serverType: 'Azure',
          username: 'azure-user',
          password: 'azure-pass',
          additionalConfig: '{"customField":"value"}',
        };
        apiClient.post.mockResolvedValue({ data: { id: 12 } });
        apiClient.get.mockResolvedValue({ data: mockCredentialMetadata });

        await store.dispatch('credentials/saveCredentials', credentialData);

        expect(apiClient.post).toHaveBeenCalledWith('/credentials', {
          serverId: 5,
          serverType: 'Azure',
          username: 'azure-user',
          password: 'azure-pass',
          additionalConfig: '{"customField":"value"}',
        });
      });

      it('should handle error when saving credentials', async () => {
        const credentialData = {
          serverId: 1,
          serverType: 'Kafka',
          username: 'kafka-user',
          password: 'kafka-pass',
        };
        const error = createMockError('Duplicate credentials');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('credentials/saveCredentials', credentialData)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('CredentialsStore', 'saving credentials', error);
      });
    });

    describe('testConnection', () => {
      it('should test connection successfully', async () => {
        const testData = {
          serverId: 1,
          serverType: 'Kafka',
          username: 'kafka-user',
          password: 'kafka-pass',
        };
        apiClient.post.mockResolvedValue({ data: { success: true, message: 'Connection successful' } });

        const result = await store.dispatch('credentials/testConnection', testData);

        expect(apiClient.post).toHaveBeenCalledWith('/credentials/test', {
          serverId: 1,
          serverType: 'Kafka',
          username: 'kafka-user',
          password: 'kafka-pass',
          additionalConfig: null,
        });
        expect(logger.success).toHaveBeenCalledWith('Connection test successful');
        expect(result).toEqual({ success: true, message: 'Connection successful' });
      });

      it('should convert AWS sessionToken to additionalConfig JSON', async () => {
        const testData = {
          serverId: 4,
          serverType: 'AWS',
          username: 'AKIAIOSFODNN7EXAMPLE',
          password: 'secret-key',
          sessionToken: 'session-token-123',
        };
        apiClient.post.mockResolvedValue({ data: { success: true } });

        await store.dispatch('credentials/testConnection', testData);

        expect(apiClient.post).toHaveBeenCalledWith('/credentials/test', {
          serverId: 4,
          serverType: 'AWS',
          username: 'AKIAIOSFODNN7EXAMPLE',
          password: 'secret-key',
          additionalConfig: JSON.stringify({ SessionToken: 'session-token-123' }),
        });
      });

      it('should show warning when connection test fails', async () => {
        const testData = {
          serverId: 1,
          serverType: 'Kafka',
          username: 'kafka-user',
          password: 'wrong-pass',
        };
        apiClient.post.mockResolvedValue({ data: { success: false, message: 'Authentication failed' } });

        await store.dispatch('credentials/testConnection', testData);

        expect(logger.warning).toHaveBeenCalledWith('Authentication failed');
      });

      it('should handle error when testing connection', async () => {
        const testData = {
          serverId: 1,
          serverType: 'Kafka',
          username: 'kafka-user',
          password: 'kafka-pass',
        };
        const error = createMockError('Network error');
        apiClient.post.mockRejectedValue(error);

        await expect(store.dispatch('credentials/testConnection', testData)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('CredentialsStore', 'testing connection', error);
      });
    });

    describe('updateCredentials', () => {
      it('should update credentials successfully', async () => {
        const updateData = {
          credentialId: 1,
          serverId: 4,
          serverType: 'AWS',
          username: 'updated-user',
          password: 'updated-pass',
        };
        apiClient.put.mockResolvedValue({ data: { success: true } });
        apiClient.get.mockResolvedValue({ data: mockCredentialMetadata });

        const result = await store.dispatch('credentials/updateCredentials', updateData);

        expect(apiClient.put).toHaveBeenCalledWith(`/credentials/${updateData.credentialId}`, {
          username: 'updated-user',
          password: 'updated-pass',
          additionalConfig: undefined,
        });
        expect(apiClient.get).toHaveBeenCalledWith(`/credentials/${updateData.serverId}/${updateData.serverType}`);
        expect(logger.success).toHaveBeenCalledWith('Credentials updated successfully');
        expect(result).toEqual({ success: true });
      });

      it('should convert AWS sessionToken to additionalConfig JSON', async () => {
        const updateData = {
          credentialId: 1,
          serverId: 4,
          serverType: 'AWS',
          username: 'updated-user',
          password: 'updated-pass',
          sessionToken: 'new-session-token',
        };
        apiClient.put.mockResolvedValue({ data: { success: true } });
        apiClient.get.mockResolvedValue({ data: mockCredentialMetadata });

        await store.dispatch('credentials/updateCredentials', updateData);

        expect(apiClient.put).toHaveBeenCalledWith(`/credentials/${updateData.credentialId}`, {
          username: 'updated-user',
          password: 'updated-pass',
          additionalConfig: JSON.stringify({ SessionToken: 'new-session-token' }),
        });
      });

      it('should handle error when updating credentials', async () => {
        const updateData = {
          credentialId: 1,
          serverId: 4,
          serverType: 'AWS',
          username: 'updated-user',
          password: 'updated-pass',
        };
        const error = createMockError('Credential not found', 404);
        apiClient.put.mockRejectedValue(error);

        await expect(store.dispatch('credentials/updateCredentials', updateData)).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('CredentialsStore', 'updating credentials', error);
      });
    });

    describe('deleteCredentials', () => {
      it('should delete credentials successfully', async () => {
        const serverType = 'AWS';
        const serverId = 4;
        const credentialId = 1;
        store.state.credentials.credentialMetadata = mockCredentialsMap;

        apiClient.delete.mockResolvedValue({ data: { success: true } });

        const result = await store.dispatch('credentials/deleteCredentials', {
          credentialId,
          serverType,
          serverId,
        });

        expect(apiClient.delete).toHaveBeenCalledWith(`/credentials/${credentialId}`);
        expect(logger.success).toHaveBeenCalledWith('Credentials deleted successfully');
        expect(result).toEqual({ success: true });
        const key = `${serverType}-${serverId}`;
        expect(store.state.credentials.credentialMetadata[key]).toBeUndefined();
      });

      it('should handle error when deleting credentials', async () => {
        const credentialId = 1;
        const serverType = 'AWS';
        const serverId = 4;
        const error = createMockError('Credential not found', 404);
        apiClient.delete.mockRejectedValue(error);

        await expect(
          store.dispatch('credentials/deleteCredentials', { credentialId, serverType, serverId })
        ).rejects.toThrow();
        expect(logger.handleApiError).toHaveBeenCalledWith('CredentialsStore', 'deleting credentials', error);
      });
    });
  });
});
