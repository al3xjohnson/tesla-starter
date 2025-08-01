# Claude Rules for TeslaStarter.Api

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

## Controller Conventions

- Inherit from `ApiControllerBase` for common functionality
- Use proper route attributes with API versioning
- Include XML documentation for all public endpoints
- Use appropriate HTTP verbs (GET, POST, PUT, DELETE)
- Return consistent response types using ActionResult<T>