# Vehicle Domain - Technical Product Requirements Document

## Overview

The Vehicle domain manages Tesla vehicle instances linked to user accounts. It represents the relationship between a Tesla account and specific vehicles, tracking their state and synchronization status.

## Domain Model

### Vehicle Aggregate Root

The `Vehicle` entity serves as the aggregate root for the vehicle domain with the following properties:

- **VehicleId**: Strongly-typed identifier (GUID-based)
- **TeslaAccountId**: Reference to the owning Tesla account
- **VehicleIdentifier**: Tesla's unique vehicle identifier (VIN or vehicle_id)
- **DisplayName**: Optional user-friendly name for the vehicle
- **LinkedAt**: Timestamp when vehicle was linked to the account
- **LastSyncedAt**: Timestamp of last successful data synchronization
- **IsActive**: Whether the vehicle is currently active

### Value Objects

#### VehicleId
- Strongly-typed identifier using GUID
- Generated using `VehicleId.New()`
- Supports empty value for validation

#### TeslaAccountId (Shared)
- References the Tesla account that owns this vehicle
- Defined in Common.Domain
- Maximum 100 characters
- Cannot be empty

### Domain Events

The Vehicle aggregate publishes the following domain events:

1. **VehicleLinkedDomainEvent**
   - Triggered when a vehicle is linked to a Tesla account
   - Contains: VehicleId, TeslaAccountId, VehicleIdentifier, DisplayName
   - Published on creation only

## Business Rules

### Vehicle Linking
- Vehicle identifier is required and cannot be empty
- Vehicle identifier is trimmed of whitespace
- Display name is optional and trimmed if provided
- Automatically sets linked timestamp
- New vehicles are active by default
- Multiple users can have the same vehicle identifier (shared vehicles)

### Display Name Management
- Can be updated at any time
- Automatically trimmed of whitespace
- Can be set to null
- No domain event for display name changes (not business-critical)

### Synchronization Tracking
- Records timestamp of successful API syncs
- Can be called multiple times
- Updates overwrite previous sync time
- Used for monitoring API health and usage

### Activation State
- Vehicles can be deactivated (soft delete)
- Cannot deactivate already inactive vehicles
- Cannot reactivate already active vehicles
- Maintains data integrity for historical records

## Repository Interface

The `IVehicleRepository` interface provides:

```csharp
public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(VehicleId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetByTeslaAccountIdAsync(TeslaAccountId teslaAccountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetActiveVehiclesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(VehicleId id, CancellationToken cancellationToken = default);
    void Add(Vehicle vehicle);
    void Update(Vehicle vehicle);
    void Remove(Vehicle vehicle);
}
```

## Use Cases

### 1. Link New Vehicle
- Receive vehicle data from Tesla API
- Create vehicle with Tesla account reference
- Set optional display name
- Publish VehicleLinkedDomainEvent

### 2. Update Vehicle Display Name
- Allow users to personalize vehicle names
- No validation beyond trimming
- No event needed (UI concern only)

### 3. Record Synchronization
- Update after successful Tesla API calls
- Track for monitoring and debugging
- Identify stale data

### 4. Deactivate Vehicle
- Soft delete when vehicle no longer accessible
- Preserve historical data
- Prevent operations on inactive vehicles

### 5. Reactivate Vehicle
- Restore previously deactivated vehicles
- Handle re-appearing vehicles in Tesla account

## Multi-User Vehicle Scenarios

The design explicitly supports multiple users having the same vehicle identifier:

```csharp
// Different users can link the same physical vehicle
Vehicle user1Vehicle = Vehicle.Link(teslaAccount1, "5YJ3E1EA1JF00001", "Family Tesla");
Vehicle user2Vehicle = Vehicle.Link(teslaAccount2, "5YJ3E1EA1JF00001", "Shared Car");
```

This enables:
- Family members sharing a vehicle
- Fleet management scenarios
- Ownership transfers

## Data Integrity

### Invariants
1. Active vehicles must have valid Tesla account reference
2. Vehicle identifier cannot be changed after creation
3. Deactivated vehicles retain all historical data
4. Each vehicle has unique VehicleId regardless of identifier

### Consistency Rules
1. Vehicle operations require active status
2. Sync timestamps must be after linked timestamp
3. Display names are user-specific (not from Tesla API)

## Performance Considerations

1. **Tesla Account Queries**: Index on TeslaAccountId for user's vehicles
2. **Active Vehicle Filter**: Composite index on IsActive + TeslaAccountId
3. **Sync Monitoring**: Index on LastSyncedAt for stale data queries
4. **Vehicle Identifier**: Not unique indexed (supports sharing)

## Integration Points

### Tesla API
- Vehicle identifier comes from Tesla API
- Initial display name can use Tesla's vehicle name
- Sync status tracks API call success
- Handle vehicle removal from Tesla account

### User Domain
- Links through TeslaAccountId
- User's Tesla account must be active
- Cascade deactivation when account unlinked

### Future Gamification
- Active status determines game participation
- Sync frequency affects pet happiness
- Display name shown in game UI
- Vehicle stats feed game mechanics

## Security Considerations

1. **Vehicle Identifier**: May contain VIN; handle as PII
2. **Access Control**: Verify Tesla account ownership
3. **Soft Delete**: Maintain audit trail
4. **Display Names**: Sanitize user input

## Future Enhancements

1. **Vehicle Metadata**: Store model, color, options
2. **Sync History**: Track detailed sync logs
3. **Vehicle Statistics**: Aggregate efficiency data
4. **Sharing Features**: Explicit vehicle sharing between users
5. **Vehicle Images**: Custom or Tesla-provided images
6. **Maintenance Tracking**: Service history integration