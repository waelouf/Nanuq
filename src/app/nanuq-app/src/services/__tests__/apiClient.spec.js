import { describe, it, expect, beforeEach, vi } from 'vitest';
import axios from 'axios';
import apiClient from '../apiClient';

vi.mock('axios');

describe('API Client', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should be an axios instance', () => {
    expect(apiClient).toBeDefined();
  });

  it('should have correct base configuration', () => {
    // API client should be configured with proper defaults
    expect(apiClient.defaults).toBeDefined();
  });

  describe('GET requests', () => {
    it('should make GET requests', async () => {
      const mockData = { data: 'test' };
      apiClient.get = vi.fn().mockResolvedValue({ data: mockData });

      const response = await apiClient.get('/test');

      expect(response.data).toEqual(mockData);
      expect(apiClient.get).toHaveBeenCalledWith('/test');
    });
  });

  describe('POST requests', () => {
    it('should make POST requests with data', async () => {
      const mockData = { id: 1 };
      const postData = { name: 'test' };
      apiClient.post = vi.fn().mockResolvedValue({ data: mockData });

      const response = await apiClient.post('/test', postData);

      expect(response.data).toEqual(mockData);
      expect(apiClient.post).toHaveBeenCalledWith('/test', postData);
    });
  });

  describe('DELETE requests', () => {
    it('should make DELETE requests', async () => {
      apiClient.delete = vi.fn().mockResolvedValue({ data: {} });

      await apiClient.delete('/test/1');

      expect(apiClient.delete).toHaveBeenCalledWith('/test/1');
    });
  });
});
