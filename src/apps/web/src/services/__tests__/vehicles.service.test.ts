import { describe, it, expect, vi, beforeEach } from 'vitest';
import { vehiclesService } from '../vehicles.service';
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

describe('VehiclesService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getVehicle', () => {
    it('should call apiClient.get with correct endpoint', async () => {
      const vehicleId = 'vehicle-123';
      const mockVehicle = {
        id: vehicleId,
        displayName: 'Model 3',
        vin: 'VIN123456789',
        state: 'online',
        chargeState: {
          batteryLevel: 75,
          chargingState: 'Charging',
          timeToFullCharge: 45,
          chargeRate: 32,
          chargerVoltage: 240,
          chargerPower: 7.7,
        },
      };
      const mockResult = { success: true as const, data: mockVehicle };
      vi.mocked(apiClient.get).mockResolvedValue(mockResult);

      const result = await vehiclesService.getVehicle(vehicleId);

      expect(apiClient.get).toHaveBeenCalledWith(`/api/v1/vehicles/${vehicleId}`);
      expect(result).toEqual(mockResult);
    });
  });

  describe('getVehiclesByTeslaAccount', () => {
    it('should call apiClient.get with correct endpoint', async () => {
      const teslaAccountId = 'tesla-account-123';
      const mockVehicles = [
        {
          id: 'vehicle-1',
          displayName: 'Model 3',
          vin: 'VIN111',
          state: 'online',
          chargeState: {
            batteryLevel: 80,
            chargingState: 'Disconnected',
            timeToFullCharge: 0,
            chargeRate: 0,
            chargerVoltage: 0,
            chargerPower: 0,
          },
        },
        {
          id: 'vehicle-2',
          displayName: 'Model Y',
          vin: 'VIN222',
          state: 'asleep',
          chargeState: {
            batteryLevel: 60,
            chargingState: 'Disconnected',
            timeToFullCharge: 0,
            chargeRate: 0,
            chargerVoltage: 0,
            chargerPower: 0,
          },
        },
      ];
      const mockResult = { success: true as const, data: mockVehicles };
      vi.mocked(apiClient.get).mockResolvedValue(mockResult);

      const result = await vehiclesService.getVehiclesByTeslaAccount(teslaAccountId);

      expect(apiClient.get).toHaveBeenCalledWith(`/api/v1/vehicles/tesla-account/${teslaAccountId}`);
      expect(result).toEqual(mockResult);
    });
  });

  describe('linkVehicle', () => {
    it('should call apiClient.post with correct endpoint and data', async () => {
      const linkRequest = {
        teslaAccountId: 'tesla-account-123',
        vehicleIdentifier: 'tesla-vehicle-456',
        displayName: 'My Model 3',
      };
      const mockVehicle = {
        id: 'vehicle-new',
        displayName: linkRequest.displayName,
        vin: 'VIN789012345',
        state: 'online',
        chargeState: {
          batteryLevel: 90,
          chargingState: 'Complete',
          timeToFullCharge: 0,
          chargeRate: 0,
          chargerVoltage: 0,
          chargerPower: 0,
        },
      };
      const mockResult = { success: true as const, data: mockVehicle };
      vi.mocked(apiClient.post).mockResolvedValue(mockResult);

      const result = await vehiclesService.linkVehicle(linkRequest);

      expect(apiClient.post).toHaveBeenCalledWith('/api/v1/vehicles', linkRequest);
      expect(result).toEqual(mockResult);
    });
  });

  describe('updateVehicle', () => {
    it('should call apiClient.put with correct endpoint and data', async () => {
      const vehicleId = 'vehicle-123';
      const updateRequest = {
        displayName: 'Updated Model 3',
        state: 'asleep' as const,
        chargeState: {
          batteryLevel: 85,
          chargingState: 'Disconnected' as const,
          timeToFullCharge: 0,
          chargeRate: 0,
          chargerVoltage: 0,
          chargerPower: 0,
        },
      };
      const mockVehicle = {
        id: vehicleId,
        vin: 'VIN123456789',
        ...updateRequest,
      };
      const mockResult = { success: true as const, data: mockVehicle };
      vi.mocked(apiClient.put).mockResolvedValue(mockResult);

      const result = await vehiclesService.updateVehicle(vehicleId, updateRequest);

      expect(apiClient.put).toHaveBeenCalledWith(`/api/v1/vehicles/${vehicleId}`, updateRequest);
      expect(result).toEqual(mockResult);
    });
  });

  describe('unlinkVehicle', () => {
    it('should call apiClient.delete with correct endpoint', async () => {
      const vehicleId = 'vehicle-123';
      const mockResult = { success: true as const, data: undefined };
      vi.mocked(apiClient.delete).mockResolvedValue(mockResult);

      const result = await vehiclesService.unlinkVehicle(vehicleId);

      expect(apiClient.delete).toHaveBeenCalledWith(`/api/v1/vehicles/${vehicleId}`);
      expect(result).toEqual(mockResult);
    });
  });
});