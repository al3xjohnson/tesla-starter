import { describe, it, expect, vi, beforeEach } from 'vitest';
import { usersService } from '../users.service';
import { apiClient } from '../api-client';

// Mock the api-client
vi.mock('../api-client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe('UsersService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getUser', () => {
    it('should call apiClient.get with correct endpoint', async () => {
      const userId = 'user-123';
      const mockUser = { id: userId, email: 'test@example.com', displayName: 'Test User' };
      const mockResult = { success: true as const, data: mockUser };
      vi.mocked(apiClient.get).mockResolvedValue(mockResult);

      const result = await usersService.getUser(userId);

      expect(apiClient.get).toHaveBeenCalledWith(`/api/v1/users/${userId}`);
      expect(result).toEqual(mockResult);
    });
  });

  describe('getUserByExternalId', () => {
    it('should call apiClient.get with correct endpoint', async () => {
      const externalId = 'ext-123';
      const mockUser = { id: 'user-123', email: 'test@example.com', displayName: 'Test User' };
      const mockResult = { success: true as const, data: mockUser };
      vi.mocked(apiClient.get).mockResolvedValue(mockResult);

      const result = await usersService.getUserByExternalId(externalId);

      expect(apiClient.get).toHaveBeenCalledWith(`/api/v1/users/external/${externalId}`);
      expect(result).toEqual(mockResult);
    });
  });

  describe('createUser', () => {
    it('should call apiClient.post with correct endpoint and data', async () => {
      const createRequest = {
        email: 'newuser@example.com',
        displayName: 'New User',
        externalId: 'ext-456',
      };
      const mockUser = { 
        id: 'user-456', 
        ...createRequest,
        createdAt: new Date().toISOString(),
      };
      const mockResult = { success: true as const, data: mockUser };
      vi.mocked(apiClient.post).mockResolvedValue(mockResult);

      const result = await usersService.createUser(createRequest);

      expect(apiClient.post).toHaveBeenCalledWith('/api/v1/users', createRequest);
      expect(result).toEqual(mockResult);
    });
  });

  describe('updateProfile', () => {
    it('should call apiClient.put with correct endpoint and data', async () => {
      const userId = 'user-123';
      const updateRequest = {
        displayName: 'Updated Name',
        preferredUnits: 'metric' as const,
      };
      const mockUser = { 
        id: userId, 
        email: 'test@example.com',
        displayName: updateRequest.displayName,
        preferredUnits: updateRequest.preferredUnits,
      };
      const mockResult = { success: true as const, data: mockUser };
      vi.mocked(apiClient.put).mockResolvedValue(mockResult);

      const result = await usersService.updateProfile(userId, updateRequest);

      expect(apiClient.put).toHaveBeenCalledWith(`/api/v1/users/${userId}/profile`, updateRequest);
      expect(result).toEqual(mockResult);
    });
  });

  describe('linkTeslaAccount', () => {
    it('should call apiClient.post with correct endpoint and data', async () => {
      const userId = 'user-123';
      const linkRequest = {
        teslaAccountId: 'tesla-account-123',
        refreshToken: 'refresh-token-123',
        accessToken: 'access-token-123',
        expiresAt: new Date(Date.now() + 3600000).toISOString(),
      };
      const mockUser = { 
        id: userId, 
        email: 'test@example.com',
        displayName: 'Test User',
        hasTeslaAccount: true,
      };
      const mockResult = { success: true as const, data: mockUser };
      vi.mocked(apiClient.post).mockResolvedValue(mockResult);

      const result = await usersService.linkTeslaAccount(userId, linkRequest);

      expect(apiClient.post).toHaveBeenCalledWith(`/api/v1/users/${userId}/tesla-account`, linkRequest);
      expect(result).toEqual(mockResult);
    });
  });

  describe('unlinkTeslaAccount', () => {
    it('should call apiClient.delete with correct endpoint', async () => {
      const userId = 'user-123';
      const mockUser = { 
        id: userId, 
        email: 'test@example.com',
        displayName: 'Test User',
        hasTeslaAccount: false,
      };
      const mockResult = { success: true as const, data: mockUser };
      vi.mocked(apiClient.delete).mockResolvedValue(mockResult);

      const result = await usersService.unlinkTeslaAccount(userId);

      expect(apiClient.delete).toHaveBeenCalledWith(`/api/v1/users/${userId}/tesla-account`);
      expect(result).toEqual(mockResult);
    });
  });

  describe('recordLogin', () => {
    it('should call apiClient.post with correct endpoint', async () => {
      const userId = 'user-123';
      const mockUser = { 
        id: userId, 
        email: 'test@example.com',
        displayName: 'Test User',
        lastLoginAt: new Date().toISOString(),
      };
      const mockResult = { success: true as const, data: mockUser };
      vi.mocked(apiClient.post).mockResolvedValue(mockResult);

      const result = await usersService.recordLogin(userId);

      expect(apiClient.post).toHaveBeenCalledWith(`/api/v1/users/${userId}/login`);
      expect(result).toEqual(mockResult);
    });
  });
});