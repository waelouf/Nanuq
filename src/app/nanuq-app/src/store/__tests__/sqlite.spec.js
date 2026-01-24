import { describe, it, expect, beforeEach, vi } from 'vitest';
import { createStore } from 'vuex';
import sqliteModule from '../sqlite';
import apiClient from '@/services/apiClient';

vi.mock('@/services/apiClient');

describe('SQLite Store', () => {
  let store;

  beforeEach(() => {
    store = createStore({
      modules: {
        sqlite: sqliteModule,
      },
    });
    vi.clearAllMocks();
  });

  describe('State', () => {
    it('should initialize with empty arrays', () => {
      expect(store.state.sqlite.kafkaServers).toEqual([]);
      expect(store.state.sqlite.redisServers).toEqual([]);
    });
  });

  describe('Mutations', () => {
    it('should update kafka servers list', () => {
      const servers = [
        { id: 1, name: 'server1', url: 'localhost:9092' },
        { id: 2, name: 'server2', url: 'localhost:9093' },
      ];

      store.commit('sqlite/loadKafkaServers', servers);

      expect(store.state.sqlite.kafkaServers).toEqual(servers);
    });

    it('should update redis servers list', () => {
      const servers = [
        { id: 1, name: 'server1', url: 'localhost:6379' },
      ];

      store.commit('sqlite/loadRedisServers', servers);

      expect(store.state.sqlite.redisServers).toEqual(servers);
    });

    it('should update rabbitmq servers list', () => {
      const servers = [
        { id: 1, name: 'rabbitmq-server', host: 'localhost', port: 5672 },
      ];

      store.commit('sqlite/loadRabbitMQServers', servers);

      expect(store.state.sqlite.rabbitMQServers).toEqual(servers);
    });

    it('should update aws servers list', () => {
      const servers = [
        { id: 1, name: 'aws-account', region: 'us-east-1' },
      ];

      store.commit('sqlite/loadAwsServers', servers);

      expect(store.state.sqlite.awsServers).toEqual(servers);
    });

    it('should update azure servers list', () => {
      const servers = [
        { id: 1, name: 'azure-servicebus', namespace: 'test-ns', region: 'eastus' },
      ];

      store.commit('sqlite/loadAzureServers', servers);

      expect(store.state.sqlite.azureServers).toEqual(servers);
    });
  });

  describe('Actions', () => {
    it('should load kafka servers successfully', async () => {
      const mockServers = [
        { id: 1, name: 'server1', url: 'localhost:9092' },
      ];

      apiClient.get.mockResolvedValue({ data: mockServers });

      await store.dispatch('sqlite/loadKafkaServers');

      expect(apiClient.get).toHaveBeenCalledWith('/sqlite/kafka');
      expect(store.state.sqlite.kafkaServers).toEqual(mockServers);
    });

    it('should add kafka server successfully', async () => {
      const newServer = { name: 'new-server', url: 'localhost:9094' };

      apiClient.post.mockResolvedValue({ data: newServer });

      await store.dispatch('sqlite/addKafkaServer', newServer);

      expect(apiClient.post).toHaveBeenCalledWith('/sqlite/kafka', newServer);
    });

    it('should delete kafka server successfully', async () => {
      const serverId = 1;

      apiClient.delete.mockResolvedValue({ data: {} });

      await store.dispatch('sqlite/deleteKafkaServer', serverId);

      expect(apiClient.delete).toHaveBeenCalledWith(`/sqlite/kafka/${serverId}`);
    });

    // ==================== Redis Server Actions ====================

    it('should load redis servers successfully', async () => {
      const mockServers = [
        { id: 2, name: 'redis-server', serverUrl: 'localhost:6379' },
      ];

      apiClient.get.mockResolvedValue({ data: mockServers });

      await store.dispatch('sqlite/loadRedisServers');

      expect(apiClient.get).toHaveBeenCalledWith('/sqlite/redis');
      expect(store.state.sqlite.redisServers).toEqual(mockServers);
    });

    it('should add redis server successfully', async () => {
      const newServer = { name: 'new-redis', serverUrl: 'localhost:6380' };

      apiClient.post.mockResolvedValue({ data: newServer });

      await store.dispatch('sqlite/addRedisServer', newServer);

      expect(apiClient.post).toHaveBeenCalledWith('/sqlite/redis', newServer);
    });

    it('should delete redis server successfully', async () => {
      const serverId = 2;

      apiClient.delete.mockResolvedValue({ data: {} });

      await store.dispatch('sqlite/deleteRedisServer', serverId);

      expect(apiClient.delete).toHaveBeenCalledWith(`/sqlite/redis/${serverId}`);
    });

    // ==================== RabbitMQ Server Actions ====================

    it('should load rabbitmq servers successfully', async () => {
      const mockServers = [
        { id: 3, name: 'rabbitmq-server', host: 'localhost', port: 5672 },
      ];

      apiClient.get.mockResolvedValue({ data: mockServers });

      await store.dispatch('sqlite/loadRabbitMQServers');

      expect(apiClient.get).toHaveBeenCalledWith('/sqlite/rabbitmq');
      expect(store.state.sqlite.rabbitMQServers).toEqual(mockServers);
    });

    it('should add rabbitmq server successfully', async () => {
      const newServer = { name: 'new-rabbitmq', host: 'localhost', port: 5673 };

      apiClient.post.mockResolvedValue({ data: newServer });

      await store.dispatch('sqlite/addRabbitMQServer', newServer);

      expect(apiClient.post).toHaveBeenCalledWith('/sqlite/rabbitmq', newServer);
    });

    it('should delete rabbitmq server successfully', async () => {
      const serverId = 3;

      apiClient.delete.mockResolvedValue({ data: {} });

      await store.dispatch('sqlite/deleteRabbitMQServer', serverId);

      expect(apiClient.delete).toHaveBeenCalledWith(`/sqlite/rabbitmq/${serverId}`);
    });

    // ==================== AWS Server Actions ====================

    it('should load aws servers successfully', async () => {
      const mockServers = [
        { id: 4, name: 'aws-account', region: 'us-east-1' },
      ];

      apiClient.get.mockResolvedValue({ data: mockServers });

      await store.dispatch('sqlite/loadAwsServers');

      expect(apiClient.get).toHaveBeenCalledWith('/sqlite/aws');
      expect(store.state.sqlite.awsServers).toEqual(mockServers);
    });

    it('should add aws server successfully', async () => {
      const newServer = { name: 'new-aws-account', region: 'us-west-2' };

      apiClient.post.mockResolvedValue({ data: newServer });

      await store.dispatch('sqlite/addAwsServer', newServer);

      expect(apiClient.post).toHaveBeenCalledWith('/sqlite/aws', newServer);
    });

    it('should delete aws server successfully', async () => {
      const serverId = 4;

      apiClient.delete.mockResolvedValue({ data: {} });

      await store.dispatch('sqlite/deleteAwsServer', serverId);

      expect(apiClient.delete).toHaveBeenCalledWith(`/sqlite/aws/${serverId}`);
    });

    // ==================== Azure Server Actions ====================

    it('should load azure servers successfully', async () => {
      const mockServers = [
        { id: 5, name: 'azure-servicebus', namespace: 'test-ns', region: 'eastus' },
      ];

      apiClient.get.mockResolvedValue({ data: mockServers });

      await store.dispatch('sqlite/loadAzureServers');

      expect(apiClient.get).toHaveBeenCalledWith('/sqlite/azure');
      expect(store.state.sqlite.azureServers).toEqual(mockServers);
    });

    it('should add azure server successfully', async () => {
      const newServer = { name: 'new-azure', namespace: 'new-ns', region: 'westus' };

      apiClient.post.mockResolvedValue({ data: newServer });

      await store.dispatch('sqlite/addAzureServer', newServer);

      expect(apiClient.post).toHaveBeenCalledWith('/sqlite/azure', newServer);
    });

    it('should delete azure server successfully', async () => {
      const serverId = 5;

      apiClient.delete.mockResolvedValue({ data: {} });

      await store.dispatch('sqlite/deleteAzureServer', serverId);

      expect(apiClient.delete).toHaveBeenCalledWith(`/sqlite/azure/${serverId}`);
    });
  });
});
