# Claude Rules for Common.Domain

## Domain Model Conventions

**Entity Framework Core Constructors**:

1. **Private constructors for EF Core** in Entity and Aggregate Root classes MUST be decorated with `[ExcludeFromCodeCoverage]` attribute:
   ```csharp
   [ExcludeFromCodeCoverage(Justification = "Required for EF Core")]
   private Vehicle() : base()
   {
       Name = default!;
   }
   ```

2. **Why this matters**:
   - These constructors are infrastructure concerns, not domain logic
   - They cannot be meaningfully tested without EF Core infrastructure
   - Including them in coverage metrics provides misleading information about actual business logic coverage

3. **When to apply**:
   - Any private parameterless constructor in an Entity or Aggregate Root
   - Any constructor that exists solely for ORM purposes
   - Do NOT apply to public constructors or factory methods

## Domain Events

1. **Event file organization**:
   - Domain events MUST be placed in an `Events` folder within the aggregate's directory
   - Example: `/Vehicles/Events/LocationCreatedEvent.cs`

2. **Event namespace convention**:
   - Events MUST use the parent aggregate's namespace, NOT include `.Events`
   - Correct: `namespace TeslaStarter.Domain.Vehicles;`
   - Incorrect: `namespace TeslaStarter.Domain.Vehicles.Events;`

3. **Why this matters**:
   - Keeps events logically grouped with their aggregate
   - Reduces namespace depth and complexity
   - Makes imports cleaner (only need to import the aggregate namespace)
   - Events are part of the aggregate's bounded context

## Value Objects

- Value objects should be immutable
- Override Equals and GetHashCode for proper equality comparisons
- Consider implementing IComparable when ordering is meaningful
- Use record types when appropriate for simple value objects