// User DTOs
export interface UserDto {
  id: string; // Guid from backend
  externalId: string;
  descopeUserId: string;
  email: string;
  displayName?: string;
  name: string;
  createdAt: string; // ISO 8601 DateTime string
  lastLoginAt?: string; // ISO 8601 DateTime string
  updatedAt: string; // ISO 8601 DateTime string
  teslaAccount?: TeslaAccountDto;
}

export interface TeslaAccountDto {
  teslaAccountId: string;
  linkedAt: string; // ISO 8601 DateTime string
  isActive: boolean;
  lastSyncedAt?: string; // ISO 8601 DateTime string
}

// User-specific API request types
export interface CreateUserRequest {
  externalId: string;
  email: string;
  displayName?: string;
}

export interface UpdateProfileRequest {
  displayName?: string;
}

export interface LinkTeslaAccountRequest {
  teslaAccountId: string;
}

export interface UserListResponse {
  users: UserDto[];
  totalCount: number;
}