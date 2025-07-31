import { describe, it, expect, vi, beforeEach } from 'vitest';
import { authService } from '../auth.service';
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

describe('AuthService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getCurrentUser', () => {
    it('should call apiClient.get with correct endpoint', async () => {
      const mockUser = { id: '123', email: 'test@example.com', displayName: 'Test User' };
      const mockResult = { success: true as const, data: mockUser };
      vi.mocked(apiClient.get).mockResolvedValue(mockResult);

      const result = await authService.getCurrentUser();

      expect(apiClient.get).toHaveBeenCalledWith('/auth/me');
      expect(result).toEqual(mockResult);
    });
  });

  describe('getAllUsers', () => {
    it('should call apiClient.get with correct endpoint', async () => {
      const mockUsers = [
        { id: '1', email: 'user1@example.com', displayName: 'User 1' },
        { id: '2', email: 'user2@example.com', displayName: 'User 2' },
      ];
      const mockResult = { success: true as const, data: mockUsers };
      vi.mocked(apiClient.get).mockResolvedValue(mockResult);

      const result = await authService.getAllUsers();

      expect(apiClient.get).toHaveBeenCalledWith('/auth/users');
      expect(result).toEqual(mockResult);
    });
  });

  describe('initiateTeslaAuth', () => {
    it('should call apiClient.get with correct endpoint', async () => {
      const mockResponse = { authUrl: 'https://auth.tesla.com/oauth2/v3/authorize?...' };
      const mockResult = { success: true as const, data: mockResponse };
      vi.mocked(apiClient.get).mockResolvedValue(mockResult);

      const result = await authService.initiateTeslaAuth();

      expect(apiClient.get).toHaveBeenCalledWith('/auth/tesla/authorize');
      expect(result).toEqual(mockResult);
    });
  });

  describe('handleTeslaCallback', () => {
    it('should call apiClient.post with correct endpoint and data', async () => {
      const mockRequest = { code: 'auth-code-123', state: 'state-123' };
      const mockResponse = { success: true, message: 'Tesla account linked successfully' };
      const mockResult = { success: true as const, data: mockResponse };
      vi.mocked(apiClient.post).mockResolvedValue(mockResult);

      const result = await authService.handleTeslaCallback(mockRequest);

      expect(apiClient.post).toHaveBeenCalledWith('/auth/tesla/callback', mockRequest);
      expect(result).toEqual(mockResult);
    });
  });

  describe('refreshTeslaTokens', () => {
    it('should call apiClient.post with correct endpoint', async () => {
      const mockResponse = { success: true, message: 'Tokens refreshed successfully' };
      const mockResult = { success: true as const, data: mockResponse };
      vi.mocked(apiClient.post).mockResolvedValue(mockResult);

      const result = await authService.refreshTeslaTokens();

      expect(apiClient.post).toHaveBeenCalledWith('/auth/tesla/refresh');
      expect(result).toEqual(mockResult);
    });
  });

  describe('unlinkTeslaAccount', () => {
    it('should call apiClient.delete with correct endpoint', async () => {
      const mockResponse = { success: true, message: 'Tesla account unlinked successfully' };
      const mockResult = { success: true as const, data: mockResponse };
      vi.mocked(apiClient.delete).mockResolvedValue(mockResult);

      const result = await authService.unlinkTeslaAccount();

      expect(apiClient.delete).toHaveBeenCalledWith('/auth/tesla/unlink');
      expect(result).toEqual(mockResult);
    });
  });

  describe('getMyVehicles', () => {
    it('should call apiClient.get with correct endpoint', async () => {
      const mockVehicles = [
        { 
          id: 'vehicle-1', 
          displayName: 'Model 3', 
          vin: 'VIN123', 
          state: 'online',
          chargeState: {
            batteryLevel: 80,
            chargingState: 'Disconnected',
            timeToFullCharge: 0,
            chargeRate: 0,
            chargerVoltage: 0,
            chargerPower: 0,
          }
        },
      ];
      const mockResult = { success: true as const, data: mockVehicles };
      vi.mocked(apiClient.get).mockResolvedValue(mockResult);

      const result = await authService.getMyVehicles();

      expect(apiClient.get).toHaveBeenCalledWith('/auth/vehicles');
      expect(result).toEqual(mockResult);
    });
  });

  describe('syncVehicles', () => {
    it('should call apiClient.post with correct endpoint', async () => {
      const mockResponse = { 
        syncedCount: 2, 
        vehicles: [
          { id: 'v1', displayName: 'Model 3' },
          { id: 'v2', displayName: 'Model Y' },
        ]
      };
      const mockResult = { success: true as const, data: mockResponse };
      vi.mocked(apiClient.post).mockResolvedValue(mockResult);

      const result = await authService.syncVehicles();

      expect(apiClient.post).toHaveBeenCalledWith('/auth/vehicles/sync');
      expect(result).toEqual(mockResult);
    });
  });
});