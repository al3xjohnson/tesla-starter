namespace TeslaStarter.Domain.Vehicles;

public record VehicleId(Guid Value)
{
    public static VehicleId New() => new(Guid.NewGuid());

    public static VehicleId Empty => new(Guid.Empty);

    public override string ToString() => Value.ToString();
}
