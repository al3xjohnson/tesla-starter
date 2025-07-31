using System.Diagnostics.CodeAnalysis;
using Common.Domain.Base;
using TeslaStarter.Domain.Vehicles.Events;

namespace TeslaStarter.Domain.Vehicles;

public sealed class Vehicle : AggregateRoot<VehicleId>
{
    /// <summary>
    /// The Tesla account that owns this vehicle instance
    /// </summary>
    public TeslaAccountId TeslaAccountId { get; private set; }

    /// <summary>
    /// Tesla's unique identifier for the vehicle (VIN or vehicle_id)
    /// </summary>
    public string VehicleIdentifier { get; private set; }

    /// <summary>
    /// User-friendly name for the vehicle
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// When this vehicle was linked to the account
    /// </summary>
    public DateTime LinkedAt { get; private set; }

    /// <summary>
    /// Last time we synced data from Tesla API
    /// </summary>
    public DateTime? LastSyncedAt { get; private set; }

    /// <summary>
    /// Whether this vehicle is currently active
    /// </summary>
    public bool IsActive { get; private set; }

    private Vehicle(
        VehicleId id,
        TeslaAccountId teslaAccountId,
        string vehicleIdentifier,
        string? displayName,
        DateTime linkedAt,
        DateTime? lastSyncedAt = null,
        bool isActive = true) : base(id)
    {
        TeslaAccountId = teslaAccountId;
        VehicleIdentifier = vehicleIdentifier;
        DisplayName = displayName;
        LinkedAt = linkedAt;
        LastSyncedAt = lastSyncedAt;
        IsActive = isActive;
    }

    [ExcludeFromCodeCoverage(Justification = "Needed for EF Core")]
    private Vehicle()
    {
        TeslaAccountId = default!;
        VehicleIdentifier = default!;
        LinkedAt = default;
    }

    /// <summary>
    /// Links a new vehicle to a Tesla account
    /// </summary>
    public static Vehicle Link(TeslaAccountId teslaAccountId, string vehicleIdentifier, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(vehicleIdentifier))
            throw new ArgumentException("Vehicle identifier cannot be empty", nameof(vehicleIdentifier));

        Vehicle vehicle = new(
            VehicleId.New(),
            teslaAccountId,
            vehicleIdentifier.Trim(),
            displayName?.Trim(),
            DateTime.UtcNow);

        vehicle.RaiseDomainEvent(new VehicleLinkedDomainEvent(
            vehicle.Id,
            vehicle.TeslaAccountId,
            vehicle.VehicleIdentifier,
            vehicle.DisplayName));

        return vehicle;
    }

    /// <summary>
    /// Updates the display name of the vehicle
    /// </summary>
    public void UpdateDisplayName(string? displayName)
    {
        DisplayName = displayName?.Trim();
    }

    /// <summary>
    /// Records that the vehicle data was synced
    /// </summary>
    public void RecordSync()
    {
        LastSyncedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the vehicle (soft delete)
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Vehicle is already inactive");

        IsActive = false;
    }

    /// <summary>
    /// Reactivates a previously deactivated vehicle
    /// </summary>
    public void Reactivate()
    {
        if (IsActive)
            throw new InvalidOperationException("Vehicle is already active");

        IsActive = true;
    }
}
