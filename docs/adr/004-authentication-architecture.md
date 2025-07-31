# ADR-004: Authentication Architecture

## Status
Accepted

## Context
TeslaStarter requires a robust authentication system that can:
- Handle user registration and login
- Integrate with Tesla's OAuth system
- Support future multi-tenancy if needed
- Provide secure session management
- Be easy to implement and maintain

## Decision
We will use Descope as our primary authentication provider with custom JWT validation in the backend.

### Key Components:
1. **Descope** for user authentication and session management
2. **Custom JWT validation** in ASP.NET Core for API authentication
3. **Automatic user synchronization** between Descope and our database
4. **Tesla OAuth integration** for linking Tesla accounts

## Consequences

### Positive
- **Reduced complexity**: No need to build and maintain our own auth system
- **Enhanced security**: Descope handles security best practices
- **Quick implementation**: Pre-built UI components and flows
- **Scalability**: Can easily add SSO, MFA, and other auth methods
- **Compliance**: Descope handles GDPR, SOC2, and other compliance requirements

### Negative
- **Vendor dependency**: Reliance on Descope's availability
- **Cost**: Potential costs as user base grows
- **Limited customization**: Some constraints on auth flow customization

### Neutral
- **Learning curve**: Team needs to understand Descope's SDK and API
- **Migration complexity**: If we need to switch providers in the future

## Implementation Details

### Backend Integration
```csharp
// Custom authentication handler
public class DescopeAuthenticationHandler : AuthenticationHandler<DescopeAuthenticationOptions>
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Extract and validate JWT token
        // Sync user data with local database
        // Return authenticated principal
    }
}
```

### Frontend Integration
```typescript
// React Context for auth state
const AuthContext = createContext<AuthState>({
    isAuthenticated: false,
    user: null,
    // ... other auth state
});

// Protected routes
<ProtectedRoute>
    <Dashboard />
</ProtectedRoute>
```

### User Synchronization Flow
1. User authenticates with Descope
2. Backend validates the session token
3. User record created/updated in local database
4. User can then link Tesla account via OAuth

## Alternatives Considered

### Auth0
- **Pros**: Well-established, extensive documentation
- **Cons**: More expensive, complex setup for our needs

### Firebase Auth
- **Pros**: Good integration with other Google services
- **Cons**: Vendor lock-in to Google ecosystem

### Custom JWT Implementation
- **Pros**: Full control, no vendor dependency
- **Cons**: Security risks, maintenance burden, longer development time

### Supabase Auth
- **Pros**: Open source option available, integrated with PostgreSQL
- **Cons**: Would require using entire Supabase stack

## References
- [Descope Documentation](https://docs.descope.com)
- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JWT Best Practices](https://datatracker.ietf.org/doc/html/rfc8725)