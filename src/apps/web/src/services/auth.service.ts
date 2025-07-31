import { apiClient, ApiResult } from './api-client';
import { 
  UserDto, 
  TeslaAuthResponse, 
  TeslaCallbackRequest, 
  VehicleDto, 
  ApiSuccessResponse, 
  SyncVehiclesResponse 
} from '@teslastarter/api-contracts';

export class AuthService {
  async getCurrentUser(): Promise<ApiResult<UserDto>> {
    return apiClient.get<UserDto>('/auth/me');
  }

  async getAllUsers(): Promise<ApiResult<UserDto[]>> {
    return apiClient.get<UserDto[]>('/auth/users');
  }

  async initiateTeslaAuth(): Promise<ApiResult<TeslaAuthResponse>> {
    return apiClient.get<TeslaAuthResponse>('/auth/tesla/authorize');
  }

  async handleTeslaCallback(request: TeslaCallbackRequest): Promise<ApiResult<ApiSuccessResponse>> {
    return apiClient.post<ApiSuccessResponse>('/auth/tesla/callback', request);
  }

  async refreshTeslaTokens(): Promise<ApiResult<ApiSuccessResponse>> {
    return apiClient.post<ApiSuccessResponse>('/auth/tesla/refresh');
  }

  async unlinkTeslaAccount(): Promise<ApiResult<ApiSuccessResponse>> {
    return apiClient.delete<ApiSuccessResponse>('/auth/tesla/unlink');
  }

  async getMyVehicles(): Promise<ApiResult<VehicleDto[]>> {
    return apiClient.get<VehicleDto[]>('/auth/vehicles');
  }

  async syncVehicles(): Promise<ApiResult<SyncVehiclesResponse>> {
    return apiClient.post<SyncVehiclesResponse>('/auth/vehicles/sync');
  }
}

export const authService = new AuthService();