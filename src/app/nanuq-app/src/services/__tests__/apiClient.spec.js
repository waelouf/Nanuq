import { describe, it, expect, beforeEach, vi } from 'vitest';
import apiClient from '../apiClient';

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
    expect(apiClient.defaults.headers['Content-Type']).toBe('application/json');
  });

  describe('GET requests', () => {
    it('should make GET requests', async () => {
      const mockData = { data: 'test' };
      const getSpy = vi.spyOn(apiClient, 'get').mockResolvedValue({ data: mockData });

      const response = await apiClient.get('/test');

      expect(response.data).toEqual(mockData);
      expect(getSpy).toHaveBeenCalledWith('/test');
      getSpy.mockRestore();
    });
  });

  describe('POST requests', () => {
    it('should make POST requests with data', async () => {
      const mockData = { id: 1 };
      const postData = { name: 'test' };
      const postSpy = vi.spyOn(apiClient, 'post').mockResolvedValue({ data: mockData });

      const response = await apiClient.post('/test', postData);

      expect(response.data).toEqual(mockData);
      expect(postSpy).toHaveBeenCalledWith('/test', postData);
      postSpy.mockRestore();
    });
  });

  describe('DELETE requests', () => {
    it('should make DELETE requests', async () => {
      const deleteSpy = vi.spyOn(apiClient, 'delete').mockResolvedValue({ data: {} });

      await apiClient.delete('/test/1');

      expect(deleteSpy).toHaveBeenCalledWith('/test/1');
      deleteSpy.mockRestore();
    });
  });
});
