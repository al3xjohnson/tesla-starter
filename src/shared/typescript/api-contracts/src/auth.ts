import type { UserDto } from './users';

// Auth state for React context
export interface AuthState {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: UserDto | null;
  sessionToken: string | null;
}

// Re-export UserDto and TeslaAccountDto for convenience
export type { UserDto, TeslaAccountDto } from './users';

// API request/response types
export interface TeslaAuthResponse {
  authUrl: string;
  state: string;
}

export interface TeslaCallbackRequest {
  code: string;
  state: string;
}

export interface TeslaTokenResponse {
  accessToken: string;
  refreshToken: string;
  idToken: string;
  tokenType: string;
  expiresIn: number;
  expiresAt: string; // ISO 8601 DateTime string
}

export interface TeslaOAuthState {
  state: string;
  descopeUserId: string;
  createdAt: string; // ISO 8601 DateTime string
  isExpired: boolean;
}

// API response types
export interface ApiSuccessResponse {
  success: boolean;
  message?: string;
}

export interface SyncVehiclesResponse {
  success: boolean;
  syncedCount: number;
  message: string;
}

export interface ApiErrorResponse {
  error: string;
}