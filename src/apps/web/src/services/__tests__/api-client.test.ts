import { describe, it, expect, vi } from 'vitest';
import { ApiError, handleApiResult, useApiResult, extractValidationErrors } from '../api-client';

// Mock axios
vi.mock('axios', () => ({
  default: {
    create: vi.fn(() => ({
      get: vi.fn(),
      post: vi.fn(),
      put: vi.fn(),
      delete: vi.fn(),
      interceptors: {
        request: {
          use: vi.fn(),
        },
        response: {
          use: vi.fn(),
        },
      },
    })),
  },
}));

// Mock descope
vi.mock('@descope/react-sdk', () => ({
  getSessionToken: vi.fn(() => 'mock-token'),
}));

// Mock config
vi.mock('@/config', () => ({
  config: {
    api: {
      baseUrl: 'http://localhost:5000/api/v1',
    },
  },
}));

describe('ApiError', () => {
  it('should create an ApiError instance with correct properties', () => {
    const problemDetails = {
      title: 'Bad Request',
      status: 400,
      detail: 'Invalid input',
      errors: {
        email: ['Email is required'],
      },
    };

    const error = new ApiError(400, problemDetails, 'trace-123');

    expect(error.status).toBe(400);
    expect(error.problem).toEqual(problemDetails);
    expect(error.traceId).toBe('trace-123');
    expect(error.message).toBe('Bad Request');
    expect(error.name).toBe('ApiError');
  });

  it('should identify validation errors correctly', () => {
    const validationError = new ApiError(400, {
      title: 'Validation Error',
      status: 400,
      errors: { field: ['error'] },
    });

    const nonValidationError = new ApiError(400, {
      title: 'Bad Request',
      status: 400,
    });

    expect(validationError.isValidationError).toBe(true);
    expect(nonValidationError.isValidationError).toBe(false);
  });

  it('should get field errors', () => {
    const error = new ApiError(400, {
      title: 'Validation Error',
      status: 400,
      errors: {
        email: ['Email is required', 'Email is invalid'],
        password: ['Password is too short'],
      },
    });

    expect(error.getFieldErrors('email')).toEqual(['Email is required', 'Email is invalid']);
    expect(error.getFieldError('email')).toBe('Email is required');
    expect(error.getFieldErrors('nonexistent')).toBeUndefined();
  });

  it('should get all errors as flat array', () => {
    const error = new ApiError(400, {
      title: 'Validation Error',
      status: 400,
      errors: {
        email: ['Email is required'],
        password: ['Password is too short', 'Password must contain numbers'],
      },
    });

    expect(error.getAllErrors()).toEqual([
      'Email is required',
      'Password is too short',
      'Password must contain numbers',
    ]);
  });
});

describe('handleApiResult', () => {
  it('should call onSuccess when result is successful', async () => {
    const onSuccess = vi.fn();
    const result = { success: true as const, data: { id: 1, name: 'Test' } };

    const data = await handleApiResult(Promise.resolve(result), { onSuccess });

    expect(onSuccess).toHaveBeenCalledWith({ id: 1, name: 'Test' });
    expect(data).toEqual({ id: 1, name: 'Test' });
  });

  it('should call onError when result is error', async () => {
    const onError = vi.fn();
    const error = new ApiError(500, { title: 'Server Error', status: 500 });
    const result = { success: false as const, error };

    const data = await handleApiResult(Promise.resolve(result), { onError });

    expect(onError).toHaveBeenCalledWith(error);
    expect(data).toBeUndefined();
  });

  it('should call onValidationError for validation errors', async () => {
    const onValidationError = vi.fn();
    const errors = { email: ['Email is required'] };
    const error = new ApiError(400, { title: 'Validation Error', status: 400, errors });
    const result = { success: false as const, error };

    await handleApiResult(Promise.resolve(result), { onValidationError });

    expect(onValidationError).toHaveBeenCalledWith(errors);
  });
});

describe('useApiResult', () => {
  it('should return loading state when result is undefined', () => {
    const state = useApiResult(undefined);

    expect(state).toEqual({
      data: undefined,
      error: undefined,
      isLoading: true,
      isError: false,
      isSuccess: false,
    });
  });

  it('should return success state for successful result', () => {
    const result = { success: true as const, data: { id: 1 } };
    const state = useApiResult(result);

    expect(state).toEqual({
      data: { id: 1 },
      error: undefined,
      isLoading: false,
      isError: false,
      isSuccess: true,
    });
  });

  it('should return error state for error result', () => {
    const error = new ApiError(500, { title: 'Error', status: 500 });
    const result = { success: false as const, error };
    const state = useApiResult(result);

    expect(state).toEqual({
      data: undefined,
      error,
      isLoading: false,
      isError: true,
      isSuccess: false,
    });
  });
});

describe('extractValidationErrors', () => {
  it('should extract validation errors for form handling', () => {
    const error = new ApiError(400, {
      title: 'Validation Error',
      status: 400,
      errors: {
        email: ['Email is required', 'Email is invalid'],
        password: ['Password is too short'],
      },
    });

    const formErrors = extractValidationErrors(error);

    expect(formErrors).toEqual({
      email: 'Email is required',
      password: 'Password is too short',
    });
  });

  it('should return empty object for non-validation errors', () => {
    const error = new ApiError(500, { title: 'Server Error', status: 500 });
    const formErrors = extractValidationErrors(error);

    expect(formErrors).toEqual({});
  });
});