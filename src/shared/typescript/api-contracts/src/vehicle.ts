export interface VehicleDto {
  id: string; // Guid from backend
  teslaAccountId: string;
  vehicleIdentifier: string;
  displayName?: string;
  linkedAt: string; // ISO 8601 DateTime string
  lastSyncedAt?: string; // ISO 8601 DateTime string
  isActive: boolean;
}

// Vehicle-specific API response types
export interface CreateVehicleRequest {
  vehicleIdentifier: string;
  displayName?: string;
}

export interface UpdateVehicleRequest {
  displayName?: string;
  isActive?: boolean;
}

export interface LinkVehicleRequest {
  teslaAccountId: string;
  vehicleIdentifier: string;
  displayName?: string;
}

export interface VehicleListResponse {
  vehicles: VehicleDto[];
  totalCount: number;
}

export interface VehicleDataDto {
  batteryLevel: number;
  batteryRange: number;
  chargingState: ChargingState;
  climateOn: boolean;
  insideTemp: number;
  outsideTemp: number;
  speed: number;
  odometer: number;
  isLocked: boolean;
  isDriving: boolean;
  timestamp: string; // ISO 8601 DateTime string
}

export enum ChargingState {
  Disconnected = 'disconnected',
  Charging = 'charging',
  Complete = 'complete',
  Stopped = 'stopped'
}