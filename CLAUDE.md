# Claude Rules for secret-sauce

## Architecture Decision Records (ADRs)

When making important architecture decisions in this codebase, you should:

1. **Create a new ADR** in `docs/adr/` whenever you:
   - Choose a framework or library
   - Design a major component or system
   - Define an API structure
   - Make security-related decisions
   - Choose between different technical approaches
   - Establish coding patterns or conventions

2. **ADR Format**:
   - Use sequential numbering (001, 002, etc.)
   - Follow the template established in `docs/adr/001-record-architecture-decisions.md`
   - Include: Title, Date, Status, Context, Decision, and Consequences

3. **When to create an ADR**:
   - If the decision will affect future development
   - If someone might ask "why did we do it this way?"
   - If there were multiple viable options considered
   - If the decision has long-term implications

Remember: ADRs help future developers (including yourself) understand why certain choices were made.

## Code Style and Formatting

**IMPORTANT**: Before completing any task, you MUST:

1. **Run code formatting** using `dotnet format` to ensure all code follows the project's style guidelines
2. **Fix any warnings** related to:
   - Collection initialization simplifications
   - Using explicit type names instead of 'var'
   - Using target-typed `new()` when the variable type is explicit
   - Ordering using statements alphabetically
   - Removing unused directives

3. **Code style preferences**:
   - Prefer explicit type names over 'var' (e.g., `string name = "test"` not `var name = "test"`)
   - Use target-typed new when type is explicit (e.g., `Entity entity = new(); List<string> items = [];`)
   - Keep using statements ordered alphabetically
   - Remove any unused using directives
   - Remove any un-needed whitespace
   - Simplify collection initializations where possible

4. **Modern C# patterns**:
   - **Use expression-bodied members** for methods/properties that can be expressed as a single expression
   - **Formatting**: Place `=>` on the same line as the method signature
   - **Multi-line expressions**: Split complex expressions across lines with proper indentation
   - **Use null-coalescing with throw** for guard clauses (e.g., `GetValue() ?? throw new Exception()`)
   - **Avoid unnecessary intermediate variables** before return statements
   - **Use pattern matching** for type/null checks (e.g., `obj is Entity entity && entity.IsValid`)
   - **Simplify boolean expressions** by combining conditions with logical operators
   
   Examples:
   ```csharp
   // ✅ Good - Simple expression
   public override int GetHashCode() => Id.GetHashCode();
   
   // ✅ Good - Multi-line expression
   public override bool Equals(object? obj) =>
       obj is Entity<TId> entity && Equals(entity);
   
   // ✅ Good - Complex multi-line expression
   public override bool Equals(object? obj) =>
       obj is Enumeration otherValue &&
       GetType() == obj.GetType() &&
       Id.Equals(otherValue.Id);
   
   // ✅ Good - Null-coalescing with throw
   public static T Parse<T>(string value) => 
       GetValue(value) ?? throw new InvalidOperationException($"Invalid: {value}");
   
   // ❌ Avoid - Multi-line method body
   public override int GetHashCode()
   {
       return Id.GetHashCode();
   }
   ```

## Domain Model Conventions

**Domain Events**:

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

## API Response Format Guidelines

Follow Microsoft's official API response guidelines:

### 1. **Use ActionResult<T> for Controller Actions**
   - All controller actions should return `ActionResult<T>` for proper HTTP status code support
   - This enables automatic content negotiation and proper error handling

### 2. **Success Response Format**
   - Return data directly without wrapper envelopes
   - For single resources: return the DTO directly
   - For collections: return arrays or lists directly
   - Include appropriate HTTP status codes:
     - 200 OK: Successful GET, PUT operations
     - 201 Created: Successful POST (include Location header)
     - 204 No Content: Successful DELETE or operations with no body

### 3. **Error Response Format**
   - Use ProblemDetails (RFC 7807) for ALL error responses
   - Error responses must include:
     - `type`: URI reference to error documentation
     - `title`: Human-readable summary
     - `status`: HTTP status code
     - `detail`: Specific error details (optional)
     - `instance`: URI of the specific request
     - `traceId`: Correlation ID for debugging
   - For validation errors, include `errors` extension with field-specific errors

### 4. **Example Responses**

Success Response:
```json
// GET /api/v1/users/123
{
  "id": "123",
  "email": "user@example.com",
  "displayName": "John Doe"
}
```

Error Response (ProblemDetails):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "instance": "/api/v1/users",
  "traceId": "00-trace-id-00",
  "errors": {
    "Email": ["The Email field is required."],
    "DisplayName": ["Display name must be between 1 and 100 characters."]
  }
}
```

### 5. **Content Negotiation**
   - Support JSON as the default format
   - Use `[Produces("application/json")]` on controllers
   - Error responses use `application/problem+json` content type

### 6. **Implementation Notes**
   - GlobalExceptionHandlingMiddleware handles all unhandled exceptions
   - Model validation errors are automatically converted to ProblemDetails
   - Custom exceptions (ValidationException, NotFoundException) are mapped to appropriate ProblemDetails responses

## Best Practices to Follow

Keep commits small and focused (typically < 100 lines changed)
Write self-documenting code with clear variable/method names
Include XML documentation comments for public APIs
Follow SOLID principles and DDD patterns
Write unit tests for new functionality (ask if unsure about test scenarios)
Handle edge cases and errors gracefully
Use meaningful exception types and messages
Follow API response format guidelines (see above)

Response Format
Always maintain clear communication:

Use headers to separate different phases
Use code blocks for file changes
Use bullet points for lists
Highlight important decisions or blockers
Explain technical concepts simply

Remember: The goal is to write production-quality code while helping someone learn. Be thorough in explanations but efficient in implementation.
