import { apiClient, ApiResult } from './api-client';
import { UserDto, CreateUserRequest, UpdateProfileRequest, LinkTeslaAccountRequest } from '@teslastarter/api-contracts';

/**
 * Type-safe Users API service with enhanced error handling
 */
export class UsersService {
  /**
   * Get a user by ID
   */
  async getUser(id: string): Promise<ApiResult<UserDto>> {
    return apiClient.get<UserDto>(`/api/v1/users/${id}`);
  }

  /**
   * Get a user by external ID
   */
  async getUserByExternalId(externalId: string): Promise<ApiResult<UserDto>> {
    return apiClient.get<UserDto>(`/api/v1/users/external/${externalId}`);
  }

  /**
   * Create a new user
   */
  async createUser(request: CreateUserRequest): Promise<ApiResult<UserDto>> {
    return apiClient.post<UserDto>('/api/v1/users', request);
  }

  /**
   * Update user profile
   */
  async updateProfile(userId: string, request: UpdateProfileRequest): Promise<ApiResult<UserDto>> {
    return apiClient.put<UserDto>(`/api/v1/users/${userId}/profile`, request);
  }

  /**
   * Link Tesla account to user
   */
  async linkTeslaAccount(userId: string, request: LinkTeslaAccountRequest): Promise<ApiResult<UserDto>> {
    return apiClient.post<UserDto>(`/api/v1/users/${userId}/tesla-account`, request);
  }

  /**
   * Unlink Tesla account from user
   */
  async unlinkTeslaAccount(userId: string): Promise<ApiResult<UserDto>> {
    return apiClient.delete<UserDto>(`/api/v1/users/${userId}/tesla-account`);
  }

  /**
   * Record user login
   */
  async recordLogin(userId: string): Promise<ApiResult<UserDto>> {
    return apiClient.post<UserDto>(`/api/v1/users/${userId}/login`);
  }
}

export const usersService = new UsersService();