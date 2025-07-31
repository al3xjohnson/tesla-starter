# TypeScript API Client

This directory contains a type-safe TypeScript client for the TeslaStarter API that makes error handling and API interactions easy for front-end developers.

## Features

- **Type Safety**: Full TypeScript support with proper interfaces
- **Consistent Error Handling**: All APIs return `ApiResult<T>` for consistent error handling
- **ProblemDetails Support**: Automatically parses RFC 7807 ProblemDetails error responses
- **Validation Error Helpers**: Easy access to field-specific validation errors
- **Clean API**: Simple, consistent method signatures
- **Authentication**: Automatic token handling via Descope SDK

## Usage Examples

### Basic API Calls

```typescript
import { usersService } from '@/services/users.service';
import { handleApiResult } from '@/services/api-client';

// Simple usage with result checking
const result = await usersService.getUser('123');
if (result.success) {
  console.log('User:', result.data);
} else {
  console.error('Error:', result.error.message);
}
```

### Handling Validation Errors

```typescript
import { usersService, CreateUserRequest } from '@/services/users.service';
import { extractValidationErrors } from '@/services/api-client';

const createUser = async (userData: CreateUserRequest) => {
  const result = await usersService.createUser(userData);
  
  if (result.success) {
    // User created successfully
    setUser(result.data);
  } else if (result.error.isValidationError) {
    // Handle validation errors
    const fieldErrors = extractValidationErrors(result.error);
    setFormErrors(fieldErrors);
    
    // Or get specific field errors
    const emailError = result.error.getFieldError('Email');
    if (emailError) {
      setEmailError(emailError);
    }
  } else {
    // Handle other errors
    showNotification(result.error.message, 'error');
  }
};
```

### Using the Helper Function

```typescript
import { usersService } from '@/services/users.service';
import { handleApiResult } from '@/services/api-client';

const loadUser = async (userId: string) => {
  const user = await handleApiResult(
    usersService.getUser(userId),
    {
      onSuccess: (user) => {
        setUser(user);
        showNotification('User loaded successfully');
      },
      onError: (error) => {
        showNotification(error.message, 'error');
      },
      onValidationError: (errors) => {
        setFormErrors(errors);
      }
    }
  );
  
  return user; // Returns user data if successful, undefined if error
};
```

### React Hook Usage

```typescript
import { useState, useEffect } from 'react';
import { usersService } from '@/services/users.service';
import { useApiResult, ApiResult } from '@/services/api-client';

const UserProfile = ({ userId }: { userId: string }) => {
  const [userResult, setUserResult] = useState<ApiResult<User>>();
  
  useEffect(() => {
    const loadUser = async () => {
      const result = await usersService.getUser(userId);
      setUserResult(result);
    };
    
    loadUser();
  }, [userId]);
  
  const { data: user, error, isLoading, isError } = useApiResult(userResult);
  
  if (isLoading) return <div>Loading...</div>;
  if (isError) return <div>Error: {error?.message}</div>;
  if (!user) return <div>No user found</div>;
  
  return (
    <div>
      <h1>{user.displayName}</h1>
      <p>{user.email}</p>
    </div>
  );
};
```

### Form Handling with Validation

```typescript
import { useState } from 'react';
import { usersService, UpdateProfileRequest } from '@/services/users.service';
import { extractValidationErrors, ApiError } from '@/services/api-client';

const ProfileForm = ({ userId }: { userId: string }) => {
  const [formData, setFormData] = useState<UpdateProfileRequest>({});
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setFieldErrors({});
    
    const result = await usersService.updateProfile(userId, formData);
    
    if (result.success) {
      showNotification('Profile updated successfully');
      // Update local state, redirect, etc.
    } else {
      if (result.error.isValidationError) {
        // Set field-specific errors
        setFieldErrors(extractValidationErrors(result.error));
      } else {
        // Show general error
        showNotification(result.error.message, 'error');
      }
    }
    
    setIsSubmitting(false);
  };
  
  return (
    <form onSubmit={handleSubmit}>
      <div>
        <input
          type="email"
          value={formData.email || ''}
          onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          placeholder="Email"
        />
        {fieldErrors.Email && <span className="error">{fieldErrors.Email}</span>}
      </div>
      
      <div>
        <input
          type="text"
          value={formData.displayName || ''}
          onChange={(e) => setFormData({ ...formData, displayName: e.target.value })}
          placeholder="Display Name"
        />
        {fieldErrors.DisplayName && <span className="error">{fieldErrors.DisplayName}</span>}
      </div>
      
      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Saving...' : 'Save Profile'}
      </button>
    </form>
  );
};
```

### Working with Collections

```typescript
import { vehiclesService } from '@/services/vehicles.service';

const VehiclesList = ({ teslaAccountId }: { teslaAccountId: string }) => {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [loading, setLoading] = useState(true);
  
  useEffect(() => {
    const loadVehicles = async () => {
      const result = await vehiclesService.getVehiclesByTeslaAccount(teslaAccountId);
      
      if (result.success) {
        setVehicles(result.data);
      } else {
        console.error('Failed to load vehicles:', result.error.message);
        setVehicles([]);
      }
      
      setLoading(false);
    };
    
    loadVehicles();
  }, [teslaAccountId]);
  
  if (loading) return <div>Loading vehicles...</div>;
  
  return (
    <div>
      {vehicles.length === 0 ? (
        <p>No vehicles found</p>
      ) : (
        <ul>
          {vehicles.map(vehicle => (
            <li key={vehicle.id}>
              <h3>{vehicle.displayName}</h3>
              <p>VIN: {vehicle.vehicleIdentifier}</p>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};
```

## API Services

### Users Service (`usersService`)
- `getUser(id: string)`: Get user by ID
- `getUserByExternalId(externalId: string)`: Get user by external ID
- `createUser(request: CreateUserRequest)`: Create new user
- `updateProfile(userId: string, request: UpdateProfileRequest)`: Update user profile
- `linkTeslaAccount(userId: string, request: LinkTeslaAccountRequest)`: Link Tesla account
- `unlinkTeslaAccount(userId: string)`: Unlink Tesla account
- `recordLogin(userId: string)`: Record user login

### Vehicles Service (`vehiclesService`)
- `getVehicle(id: string)`: Get vehicle by ID
- `getVehiclesByTeslaAccount(teslaAccountId: string)`: Get vehicles for Tesla account
- `linkVehicle(request: LinkVehicleRequest)`: Link new vehicle
- `updateVehicle(vehicleId: string, request: UpdateVehicleRequest)`: Update vehicle
- `unlinkVehicle(vehicleId: string)`: Unlink vehicle

## Error Types

### ApiError
Custom error class with helpful methods:
- `isValidationError`: Check if it's a validation error (400 with field errors)
- `getFieldErrors(field: string)`: Get errors for a specific field
- `getFieldError(field: string)`: Get first error for a specific field
- `getAllErrors()`: Get all validation errors as flat array

### ProblemDetails
RFC 7807 compliant error format:
- `type`: URI reference to error documentation
- `title`: Human-readable error summary
- `status`: HTTP status code
- `detail`: Specific error details
- `instance`: URI of the request that caused the error
- `traceId`: Correlation ID for debugging
- `errors`: Field-specific validation errors (for 400 responses)

## Benefits for Front-End Teams

1. **Consistent Error Handling**: No more try/catch blocks everywhere
2. **Type Safety**: Full IntelliSense support and compile-time error checking
3. **Validation Made Easy**: Built-in helpers for form validation errors
4. **Better UX**: Easy access to user-friendly error messages
5. **Debugging**: Trace IDs for better error tracking
6. **Maintainable**: Service classes keep API calls organized