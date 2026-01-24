import { describe, it, expect, beforeEach, vi } from 'vitest';
import CredentialForm from '../CredentialForm.vue';

describe('CredentialForm.vue - Logic Tests', () => {
  let componentInstance;
  let mockStore;

  beforeEach(() => {
    // Create mock store
    mockStore = {
      getters: {
        'credentials/hasCredentials': vi.fn(() => false),
        'credentials/getMetadata': vi.fn(() => null),
      },
      dispatch: vi.fn().mockResolvedValue(),
    };

    // Create component instance with mocked dependencies
    componentInstance = {
      ...CredentialForm,
      $store: mockStore,
      $emit: vi.fn(),
      serverId: 1,
      serverType: 'Kafka',
      username: '',
      password: '',
      sessionToken: '',
      showPassword: false,
      showSessionToken: false,
      testing: false,
      saving: false,
      deleting: false,
      testResult: null,
      rules: {
        required: (value) => !!value || 'Required',
      },
    };
  });

  // ==================== Computed Properties ====================

  describe('Computed: hasExistingCredentials', () => {
    it('should return true when credentials exist', () => {
      mockStore.getters['credentials/hasCredentials'] = vi.fn(() => true);

      const result = CredentialForm.computed.hasExistingCredentials.call({
        $store: mockStore,
        serverType: 'Kafka',
        serverId: 1,
      });

      expect(result).toBe(true);
    });

    it('should return false when credentials do not exist', () => {
      mockStore.getters['credentials/hasCredentials'] = vi.fn(() => false);

      const result = CredentialForm.computed.hasExistingCredentials.call({
        $store: mockStore,
        serverType: 'Kafka',
        serverId: 1,
      });

      expect(result).toBe(false);
    });
  });

  describe('Computed: credentialMetadata', () => {
    it('should return metadata from store', () => {
      const mockMetadata = { id: 123, hasCredentials: true };
      mockStore.getters['credentials/getMetadata'] = vi.fn(() => mockMetadata);

      const result = CredentialForm.computed.credentialMetadata.call({
        $store: mockStore,
        serverType: 'Kafka',
        serverId: 1,
      });

      expect(result).toEqual(mockMetadata);
    });
  });

  describe('Computed: isTestable', () => {
    it('should return false for Redis without password', () => {
      const result = CredentialForm.computed.isTestable.call({
        serverType: 'Redis',
        username: '',
        password: '',
      });

      expect(result).toBe(false);
    });

    it('should return true for Redis with password only', () => {
      const result = CredentialForm.computed.isTestable.call({
        serverType: 'Redis',
        username: '',
        password: 'pass123',
      });

      expect(result).toBe(true);
    });

    it('should return false for Kafka with only username', () => {
      const result = CredentialForm.computed.isTestable.call({
        serverType: 'Kafka',
        username: 'user',
        password: '',
      });

      expect(result).toBe(false);
    });

    it('should return true for Kafka with both username and password', () => {
      const result = CredentialForm.computed.isTestable.call({
        serverType: 'Kafka',
        username: 'user',
        password: 'pass',
      });

      expect(result).toBe(true);
    });

    it('should return true for AWS with both username and password', () => {
      const result = CredentialForm.computed.isTestable.call({
        serverType: 'AWS',
        username: 'accessKey',
        password: 'secretKey',
      });

      expect(result).toBe(true);
    });

    it('should return false for RabbitMQ without both fields', () => {
      const result = CredentialForm.computed.isTestable.call({
        serverType: 'RabbitMQ',
        username: 'user',
        password: '',
      });

      expect(result).toBe(false);
    });
  });

  describe('Computed: isSaveable', () => {
    it('should return false for new Kafka credentials without both fields', () => {
      const result = CredentialForm.computed.isSaveable.call({
        serverType: 'Kafka',
        username: 'user',
        password: '',
        hasExistingCredentials: false,
      });

      expect(result).toBe(false);
    });

    it('should return true for new Kafka credentials with both fields', () => {
      const result = CredentialForm.computed.isSaveable.call({
        serverType: 'Kafka',
        username: 'user',
        password: 'pass',
        hasExistingCredentials: false,
      });

      expect(result).toBe(true);
    });

    it('should return true for new Redis credentials with password only', () => {
      const result = CredentialForm.computed.isSaveable.call({
        serverType: 'Redis',
        username: '',
        password: 'pass',
        hasExistingCredentials: false,
      });

      expect(result).toBe(true);
    });

    it('should return true for updating with either field filled', () => {
      const resultUsername = CredentialForm.computed.isSaveable.call({
        serverType: 'AWS',
        username: 'newUser',
        password: '',
        hasExistingCredentials: true,
      });

      const resultPassword = CredentialForm.computed.isSaveable.call({
        serverType: 'AWS',
        username: '',
        password: 'newPass',
        hasExistingCredentials: true,
      });

      expect(resultUsername).toBe(true);
      expect(resultPassword).toBe(true);
    });

    it('should return false for update with no fields filled', () => {
      const result = CredentialForm.computed.isSaveable.call({
        serverType: 'AWS',
        username: '',
        password: '',
        hasExistingCredentials: true,
      });

      expect(result).toBe(false);
    });
  });

  // ==================== Methods ====================

  describe('Methods: getUsernameLabel', () => {
    it('should return "Access Key ID" for AWS', () => {
      const result = CredentialForm.methods.getUsernameLabel.call({
        serverType: 'AWS',
      });

      expect(result).toBe('Access Key ID');
    });

    it('should return "Connection String Name (optional)" for Azure', () => {
      const result = CredentialForm.methods.getUsernameLabel.call({
        serverType: 'Azure',
      });

      expect(result).toBe('Connection String Name (optional)');
    });

    it('should return "Username (optional for Redis)" for Redis', () => {
      const result = CredentialForm.methods.getUsernameLabel.call({
        serverType: 'Redis',
      });

      expect(result).toBe('Username (optional for Redis)');
    });

    it('should return "Username (optional)" for Kafka', () => {
      const result = CredentialForm.methods.getUsernameLabel.call({
        serverType: 'Kafka',
      });

      expect(result).toBe('Username (optional)');
    });

    it('should return "Username" for RabbitMQ', () => {
      const result = CredentialForm.methods.getUsernameLabel.call({
        serverType: 'RabbitMQ',
      });

      expect(result).toBe('Username');
    });
  });

  describe('Methods: getPasswordLabel', () => {
    it('should return "Secret Access Key" for new AWS credentials', () => {
      const result = CredentialForm.methods.getPasswordLabel.call({
        serverType: 'AWS',
        hasExistingCredentials: false,
      });

      expect(result).toBe('Secret Access Key');
    });

    it('should return update label for existing AWS credentials', () => {
      const result = CredentialForm.methods.getPasswordLabel.call({
        serverType: 'AWS',
        hasExistingCredentials: true,
      });

      expect(result).toBe('Secret Access Key (leave blank to keep current)');
    });

    it('should return "Connection String" for new Azure credentials', () => {
      const result = CredentialForm.methods.getPasswordLabel.call({
        serverType: 'Azure',
        hasExistingCredentials: false,
      });

      expect(result).toBe('Connection String');
    });

    it('should return update label for existing Azure credentials', () => {
      const result = CredentialForm.methods.getPasswordLabel.call({
        serverType: 'Azure',
        hasExistingCredentials: true,
      });

      expect(result).toBe('Connection String (leave blank to keep current)');
    });

    it('should return "Password (optional)" for Kafka', () => {
      const result = CredentialForm.methods.getPasswordLabel.call({
        serverType: 'Kafka',
        hasExistingCredentials: false,
      });

      expect(result).toBe('Password (optional)');
    });

    it('should return update label for existing RabbitMQ credentials', () => {
      const result = CredentialForm.methods.getPasswordLabel.call({
        serverType: 'RabbitMQ',
        hasExistingCredentials: true,
      });

      expect(result).toBe('Password (leave blank to keep current)');
    });
  });

  describe('Methods: getPasswordRules', () => {
    it('should return empty rules for existing credentials', () => {
      const result = CredentialForm.methods.getPasswordRules.call({
        hasExistingCredentials: true,
        serverType: 'AWS',
      });

      expect(result).toEqual([]);
    });

    it('should return empty rules for Kafka new credentials', () => {
      const result = CredentialForm.methods.getPasswordRules.call({
        hasExistingCredentials: false,
        serverType: 'Kafka',
        rules: { required: (v) => !!v || 'Required' },
      });

      expect(result).toEqual([]);
    });

    it('should return empty rules for Redis new credentials', () => {
      const result = CredentialForm.methods.getPasswordRules.call({
        hasExistingCredentials: false,
        serverType: 'Redis',
        rules: { required: (v) => !!v || 'Required' },
      });

      expect(result).toEqual([]);
    });

    it('should return required rule for AWS new credentials', () => {
      const rules = CredentialForm.methods.getPasswordRules.call({
        hasExistingCredentials: false,
        serverType: 'AWS',
        rules: { required: (v) => !!v || 'Required' },
      });

      expect(rules).toHaveLength(1);
      expect(rules[0]('test')).toBe(true);
      expect(rules[0]('')).toBe('Required');
    });
  });

  describe('Methods: handleTestConnection', () => {
    it('should not test if not testable', async () => {
      const context = {
        isTestable: false,
        $store: mockStore,
      };

      await CredentialForm.methods.handleTestConnection.call(context);

      expect(mockStore.dispatch).not.toHaveBeenCalled();
    });

    it('should dispatch testConnection with correct params for Kafka', async () => {
      const context = {
        isTestable: true,
        testing: false,
        testResult: null,
        serverId: 1,
        serverType: 'Kafka',
        username: 'user',
        password: 'pass',
        sessionToken: '',
        $store: mockStore,
      };

      mockStore.dispatch.mockResolvedValue({ success: true, message: 'Connected' });

      await CredentialForm.methods.handleTestConnection.call(context);

      expect(mockStore.dispatch).toHaveBeenCalledWith('credentials/testConnection', {
        serverId: 1,
        serverType: 'Kafka',
        username: 'user',
        password: 'pass',
        sessionToken: undefined,
      });
    });

    it('should dispatch testConnection with sessionToken for AWS', async () => {
      const context = {
        isTestable: true,
        testing: false,
        testResult: null,
        serverId: 1,
        serverType: 'AWS',
        username: 'accessKey',
        password: 'secretKey',
        sessionToken: 'token123',
        $store: mockStore,
      };

      mockStore.dispatch.mockResolvedValue({ success: true, message: 'Connected' });

      await CredentialForm.methods.handleTestConnection.call(context);

      expect(mockStore.dispatch).toHaveBeenCalledWith('credentials/testConnection', {
        serverId: 1,
        serverType: 'AWS',
        username: 'accessKey',
        password: 'secretKey',
        sessionToken: 'token123',
      });
    });

    it('should handle successful test result', async () => {
      const context = {
        isTestable: true,
        testing: false,
        testResult: null,
        serverId: 1,
        serverType: 'Kafka',
        username: 'user',
        password: 'pass',
        sessionToken: '',
        $store: mockStore,
      };

      const successResult = { success: true, message: 'Connected successfully' };
      mockStore.dispatch.mockResolvedValue(successResult);

      await CredentialForm.methods.handleTestConnection.call(context);

      expect(context.testResult).toEqual(successResult);
      expect(context.testing).toBe(false);
    });

    it('should handle test connection error', async () => {
      const context = {
        isTestable: true,
        testing: false,
        testResult: null,
        serverId: 1,
        serverType: 'Kafka',
        username: 'user',
        password: 'wrongpass',
        sessionToken: '',
        $store: mockStore,
      };

      mockStore.dispatch.mockRejectedValue(new Error('Authentication failed'));

      await CredentialForm.methods.handleTestConnection.call(context);

      expect(context.testResult).toEqual({
        success: false,
        message: 'Connection test failed: Authentication failed',
      });
      expect(context.testing).toBe(false);
    });
  });

  describe('Methods: handleSave', () => {
    it('should not save if not saveable', async () => {
      const context = {
        isSaveable: false,
        $store: mockStore,
      };

      await CredentialForm.methods.handleSave.call(context);

      expect(mockStore.dispatch).not.toHaveBeenCalled();
    });

    it('should dispatch saveCredentials for new credentials', async () => {
      const context = {
        isSaveable: true,
        hasExistingCredentials: false,
        saving: false,
        serverId: 1,
        serverType: 'Kafka',
        username: 'newUser',
        password: 'newPass',
        sessionToken: '',
        $store: mockStore,
        $emit: vi.fn(),
      };

      await CredentialForm.methods.handleSave.call(context);

      expect(mockStore.dispatch).toHaveBeenCalledWith('credentials/saveCredentials', {
        serverId: 1,
        serverType: 'Kafka',
        username: 'newUser',
        password: 'newPass',
        sessionToken: undefined,
      });
      expect(context.$emit).toHaveBeenCalledWith('saved', { action: 'created' });
    });

    it('should dispatch updateCredentials for existing credentials', async () => {
      const mockMetadata = { id: 123 };
      const context = {
        isSaveable: true,
        hasExistingCredentials: true,
        credentialMetadata: mockMetadata,
        saving: false,
        serverId: 1,
        serverType: 'AWS',
        username: 'updatedKey',
        password: 'updatedSecret',
        sessionToken: 'newToken',
        $store: mockStore,
        $emit: vi.fn(),
      };

      await CredentialForm.methods.handleSave.call(context);

      expect(mockStore.dispatch).toHaveBeenCalledWith('credentials/updateCredentials', {
        credentialId: 123,
        serverId: 1,
        serverType: 'AWS',
        username: 'updatedKey',
        password: 'updatedSecret',
        sessionToken: 'newToken',
      });
      expect(context.$emit).toHaveBeenCalledWith('saved', { action: 'updated' });
    });

    it('should clear password and sessionToken after save', async () => {
      const context = {
        isSaveable: true,
        hasExistingCredentials: false,
        saving: false,
        serverId: 1,
        serverType: 'AWS',
        username: 'accessKey',
        password: 'secretKey',
        sessionToken: 'token',
        testResult: { success: true },
        $store: mockStore,
        $emit: vi.fn(),
      };

      await CredentialForm.methods.handleSave.call(context);

      expect(context.password).toBe('');
      expect(context.sessionToken).toBe('');
      expect(context.testResult).toBe(null);
      expect(context.username).toBe('accessKey'); // Username not cleared
    });

    it('should handle save errors gracefully', async () => {
      const context = {
        isSaveable: true,
        hasExistingCredentials: false,
        saving: false,
        serverId: 1,
        serverType: 'Kafka',
        username: 'user',
        password: 'pass',
        sessionToken: '',
        $store: mockStore,
        $emit: vi.fn(),
      };

      mockStore.dispatch.mockRejectedValue(new Error('Save failed'));

      await CredentialForm.methods.handleSave.call(context);

      // Error handled by store, saving state should be false
      expect(context.saving).toBe(false);
    });
  });

  describe('Methods: handleDelete', () => {
    it('should not delete if no existing credentials', async () => {
      const context = {
        hasExistingCredentials: false,
        $store: mockStore,
      };

      await CredentialForm.methods.handleDelete.call(context);

      expect(mockStore.dispatch).not.toHaveBeenCalled();
    });

    it('should dispatch deleteCredentials with correct params', async () => {
      const mockMetadata = { id: 456 };
      const context = {
        hasExistingCredentials: true,
        credentialMetadata: mockMetadata,
        deleting: false,
        serverId: 1,
        serverType: 'RabbitMQ',
        username: 'user',
        password: 'pass',
        sessionToken: '',
        testResult: null,
        $store: mockStore,
        $emit: vi.fn(),
      };

      await CredentialForm.methods.handleDelete.call(context);

      expect(mockStore.dispatch).toHaveBeenCalledWith('credentials/deleteCredentials', {
        credentialId: 456,
        serverType: 'RabbitMQ',
        serverId: 1,
      });
      expect(context.$emit).toHaveBeenCalledWith('deleted');
    });

    it('should clear all fields after delete', async () => {
      const mockMetadata = { id: 789 };
      const context = {
        hasExistingCredentials: true,
        credentialMetadata: mockMetadata,
        deleting: false,
        serverId: 1,
        serverType: 'Redis',
        username: 'redisUser',
        password: 'redisPass',
        sessionToken: 'someToken',
        testResult: { success: false, message: 'Error' },
        $store: mockStore,
        $emit: vi.fn(),
      };

      await CredentialForm.methods.handleDelete.call(context);

      expect(context.username).toBe('');
      expect(context.password).toBe('');
      expect(context.sessionToken).toBe('');
      expect(context.testResult).toBe(null);
    });

    it('should handle delete errors gracefully', async () => {
      const mockMetadata = { id: 999 };
      const context = {
        hasExistingCredentials: true,
        credentialMetadata: mockMetadata,
        deleting: false,
        serverId: 1,
        serverType: 'AWS',
        username: '',
        password: '',
        sessionToken: '',
        testResult: null,
        $store: mockStore,
        $emit: vi.fn(),
      };

      mockStore.dispatch.mockRejectedValue(new Error('Delete failed'));

      await CredentialForm.methods.handleDelete.call(context);

      // Error handled by store, deleting state should be false
      expect(context.deleting).toBe(false);
    });
  });

  describe('Methods: loadCredentialMetadata', () => {
    it('should dispatch fetchCredentialMetadata', async () => {
      const context = {
        serverId: 1,
        serverType: 'Kafka',
        $store: mockStore,
      };

      await CredentialForm.methods.loadCredentialMetadata.call(context);

      expect(mockStore.dispatch).toHaveBeenCalledWith('credentials/fetchCredentialMetadata', {
        serverType: 'Kafka',
        serverId: 1,
      });
    });

    it('should handle 404 errors silently', async () => {
      const context = {
        serverId: 1,
        serverType: 'Kafka',
        $store: mockStore,
      };

      const notFoundError = new Error('Not found');
      notFoundError.response = { status: 404 };
      mockStore.dispatch.mockRejectedValue(notFoundError);

      // Should not throw
      await expect(
        CredentialForm.methods.loadCredentialMetadata.call(context)
      ).resolves.not.toThrow();
    });
  });

  // ==================== Props Validation ====================

  describe('Props Validation', () => {
    it('should validate serverType prop', () => {
      const validator = CredentialForm.props.serverType.validator;

      expect(validator('Kafka')).toBe(true);
      expect(validator('Redis')).toBe(true);
      expect(validator('RabbitMQ')).toBe(true);
      expect(validator('AWS')).toBe(true);
      expect(validator('Azure')).toBe(true);
      expect(validator('Invalid')).toBe(false);
    });

    it('should require serverId prop', () => {
      expect(CredentialForm.props.serverId.required).toBe(true);
      expect(CredentialForm.props.serverId.type).toBe(Number);
    });

    it('should require serverType prop', () => {
      expect(CredentialForm.props.serverType.required).toBe(true);
      expect(CredentialForm.props.serverType.type).toBe(String);
    });
  });
});
