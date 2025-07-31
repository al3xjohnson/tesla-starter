import { apiClient, ApiResult } from './api-client';
import { VehicleDto, UpdateVehicleRequest, LinkVehicleRequest } from '@teslastarter/api-contracts';

/**
 * Type-safe Vehicles API service with enhanced error handling
 */
export class VehiclesService {
  /**
   * Get a vehicle by ID
   */
  async getVehicle(id: string): Promise<ApiResult<VehicleDto>> {
    return apiClient.get<VehicleDto>(`/api/v1/vehicles/${id}`);
  }

  /**
   * Get vehicles by Tesla account ID
   */
  async getVehiclesByTeslaAccount(teslaAccountId: string): Promise<ApiResult<VehicleDto[]>> {
    return apiClient.get<VehicleDto[]>(`/api/v1/vehicles/tesla-account/${teslaAccountId}`);
  }

  /**
   * Link a new vehicle
   */
  async linkVehicle(request: LinkVehicleRequest): Promise<ApiResult<VehicleDto>> {
    return apiClient.post<VehicleDto>('/api/v1/vehicles', request);
  }

  /**
   * Update vehicle details
   */
  async updateVehicle(vehicleId: string, request: UpdateVehicleRequest): Promise<ApiResult<VehicleDto>> {
    return apiClient.put<VehicleDto>(`/api/v1/vehicles/${vehicleId}`, request);
  }

  /**
   * Unlink a vehicle
   */
  async unlinkVehicle(vehicleId: string): Promise<ApiResult<void>> {
    return apiClient.delete<void>(`/api/v1/vehicles/${vehicleId}`);
  }
}

export const vehiclesService = new VehiclesService();