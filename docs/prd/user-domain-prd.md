# User Domain - Technical Product Requirements Document

## Overview

The User domain manages user accounts, authentication, and Tesla account integration within the TeslaStarter application. It follows Domain-Driven Design principles with a clear aggregate root and value objects.

## Domain Model

### User Aggregate Root

The `User` entity serves as the aggregate root for the user domain with the following properties:

- **UserId**: Strongly-typed identifier (GUID-based)
- **ExternalId**: OAuth provider's user identifier (e.g., "auth0|123")
- **Email**: User's email address (validated and normalized)
- **DisplayName**: Optional user-friendly name
- **CreatedAt**: Timestamp of account creation
- **LastLoginAt**: Timestamp of most recent login
- **TeslaAccount**: Optional linked Tesla account information

### Value Objects

#### Email
- Validates email format using regex pattern
- Normalizes to lowercase
- Immutable once created
- Maximum length validation

#### ExternalId
- Represents the OAuth provider's user identifier
- Cannot be empty or whitespace
- Maximum 200 characters
- Trimmed on creation

#### TeslaAccount (Embedded Value Object)
- **TeslaAccountId**: Tesla's account identifier
- **Email**: Tesla account email
- **LinkedAt**: When the account was linked
- **IsActive**: Whether the link is currently active
- **RefreshToken**: OAuth refresh token (nullable)
- **LastSyncedAt**: Last successful API sync timestamp

### Domain Events

The User aggregate publishes the following domain events:

1. **UserCreatedDomainEvent**
   - Triggered when a new user is created
   - Contains: UserId, ExternalId, Email

2. **UserProfileUpdatedDomainEvent**
   - Triggered when email or display name changes
   - Contains: UserId, OldEmail, NewEmail, OldDisplayName, NewDisplayName
   - Only raised if actual changes occur

3. **UserLoggedInDomainEvent**
   - Triggered on successful login
   - Contains: UserId, LoginTime

4. **TeslaAccountLinkedDomainEvent**
   - Triggered when linking a Tesla account
   - Contains: UserId, TeslaAccountId, TeslaEmail

5. **TeslaAccountUnlinkedDomainEvent**
   - Triggered when unlinking (deactivating) a Tesla account
   - Contains: UserId, TeslaAccountId

6. **TeslaAccountReactivatedDomainEvent**
   - Triggered when reactivating a previously linked account
   - Contains: UserId, TeslaAccountId

## Business Rules

### User Creation
- External ID and email are required
- Email must be valid format
- External ID cannot exceed 200 characters
- Display name is optional
- Automatically sets creation timestamp

### Profile Updates
- Email changes are validated and normalized
- Display name can be changed or removed
- Only publishes event if actual changes occur
- Preserves immutability through new object creation

### Tesla Account Management

#### Linking Rules
- Cannot link if an active Tesla account already exists
- Can link to a previously deactivated account (replaces it)
- Requires Tesla account ID and email
- Sets linked timestamp and active status

#### Unlinking Rules
- Cannot unlink if no account is linked
- Deactivates the account (soft delete)
- Clears the refresh token
- Preserves account history

#### Token Management
- Can only update token for active accounts
- Updates last synced timestamp
- Maintains token security through immutability

#### Reactivation
- Can only reactivate inactive accounts
- Preserves existing account data
- Publishes reactivation event

## Repository Interface

The `IUserRepository` interface provides:

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);
    Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetUsersWithActiveTeslaAccountsAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    void Add(User user);
    void Update(User user);
    void Remove(User user);
}
```

## Use Cases

### 1. User Registration
- Validate external ID from OAuth provider
- Check email uniqueness
- Create user with optional display name
- Publish UserCreatedDomainEvent

### 2. User Login
- Find user by external ID
- Update last login timestamp
- Publish UserLoggedInDomainEvent

### 3. Profile Management
- Update email and/or display name
- Validate email format
- Check email uniqueness (if changed)
- Publish UserProfileUpdatedDomainEvent

### 4. Tesla Account Integration
- Link Tesla account with OAuth flow
- Store refresh token securely
- Support unlinking and reactivation
- Track sync timestamps

## Security Considerations

1. **External ID**: Never expose internal UserId; use external ID for API operations
2. **Email**: Always validate and normalize before storage
3. **Refresh Token**: Store encrypted, never expose in events or logs
4. **Soft Delete**: Deactivate rather than delete for audit trail

## Performance Considerations

1. **Email Lookups**: Index on normalized email for fast queries
2. **External ID**: Primary lookup path, ensure indexed
3. **Active Accounts**: Filtered index for users with active Tesla accounts
4. **Event Sourcing**: Consider event store for audit requirements

## Future Enhancements

1. **Multi-Tesla Account Support**: Allow multiple vehicles per user
2. **Account Merging**: Handle users with multiple OAuth providers
3. **Profile Enrichment**: Additional user preferences and settings
4. **Activity Tracking**: More detailed user behavior analytics
5. **GDPR Compliance**: Right to be forgotten implementation