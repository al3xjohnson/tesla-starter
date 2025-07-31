import axios, { AxiosInstance, AxiosResponse, AxiosError } from 'axios';
import { getSessionToken } from '@descope/react-sdk';
import { config } from '@/config';

/**
 * Problem Details format (RFC 7807) returned by the API for errors
 */
export interface ProblemDetails {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}

/**
 * Custom error class for API errors with ProblemDetails
 */
export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly problem: ProblemDetails,
    public readonly traceId?: string
  ) {
    super(problem.title);
    this.name = 'ApiError';
  }

  /**
   * Check if this is a validation error (400 with field errors)
   */
  get isValidationError(): boolean {
    return this.status === 400 && !!this.problem.errors;
  }

  /**
   * Get validation errors for a specific field
   */
  getFieldErrors(field: string): string[] | undefined {
    return this.problem.errors?.[field];
  }

  /**
   * Get the first validation error for a specific field
   */
  getFieldError(field: string): string | undefined {
    return this.getFieldErrors(field)?.[0];
  }

  /**
   * Get all validation errors as a flat array
   */
  getAllErrors(): string[] {
    if (!this.problem.errors) return [];
    return Object.values(this.problem.errors).flat();
  }
}

/**
 * Result type for API responses - either success with data or error
 */
export type ApiResult<T> = 
  | { success: true; data: T }
  | { success: false; error: ApiError };

/**
 * Enhanced response wrapper that includes the result pattern
 */
export interface ApiResponse<T> extends AxiosResponse<T> {
  result: ApiResult<T>;
}

class ApiClient {
  private axiosInstance: AxiosInstance;

  constructor() {
    this.axiosInstance = axios.create({
      baseURL: config.api.baseUrl,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor to add auth token
    this.axiosInstance.interceptors.request.use(
      (config) => {
        const token = this.getAuthToken();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Response interceptor for error handling and result wrapping
    this.axiosInstance.interceptors.response.use(
      (response) => {
        // Wrap successful responses with result pattern
        const enhancedResponse = response as ApiResponse<unknown>;
        enhancedResponse.result = { success: true, data: response.data };
        return enhancedResponse;
      },
      (error: AxiosError) => {
        if (error.response?.status === 401) {
          // Only redirect if not already on login page and not making auth-related requests
          const currentPath = window.location.pathname;
          const isAuthEndpoint = error.config?.url?.includes('/auth/');
          
          if (currentPath !== '/login' && !isAuthEndpoint) {
            window.location.href = '/login';
          }
        }

        // Convert Axios error to our ApiError format
        if (error.response) {
          const problemDetails = this.extractProblemDetails(error.response);
          const apiError = new ApiError(
            error.response.status, 
            problemDetails,
            problemDetails.traceId
          );
          
          // Create enhanced response with error result
          const enhancedError = error as AxiosError & { result: ApiResult<never> };
          enhancedError.result = { success: false, error: apiError };
          return Promise.reject(enhancedError);
        }

        // Network error
        const networkError = new ApiError(0, {
          title: 'Network Error',
          status: 0,
          detail: error.message || 'Network request failed',
        });
        
        const enhancedError = error as AxiosError & { result: ApiResult<never> };
        enhancedError.result = { success: false, error: networkError };
        return Promise.reject(enhancedError);
      }
    );
  }

  /**
   * Extract ProblemDetails from error response
   */
  private extractProblemDetails(response: AxiosResponse): ProblemDetails {
    const data = response.data;
    
    // If response data follows ProblemDetails format
    if (data && typeof data === 'object' && data.title) {
      return {
        type: data.type,
        title: data.title,
        status: data.status || response.status,
        detail: data.detail,
        instance: data.instance,
        traceId: data.traceId,
        errors: data.errors,
      };
    }
    
    // Fallback for non-ProblemDetails responses
    return {
      title: response.statusText || 'Unknown Error',
      status: response.status,
      detail: typeof data === 'string' ? data : 'An error occurred',
    };
  }

  private getAuthToken(): string | null {
    try {
      const token = getSessionToken();
      return token;
    } catch {
      return null;
    }
  }

  public setAuthToken(): void {
    // Token is managed by Descope SDK
    // This method is kept for compatibility but doesn't do anything
  }

  public clearAuthToken(): void {
    // Token is managed by Descope SDK
    // This method is kept for compatibility but doesn't do anything
  }

  // Clean result-based API methods
  public async get<T = unknown>(url: string, config?: Record<string, unknown>): Promise<ApiResult<T>> {
    try {
      const response = await this.axiosInstance.get<T>(url, config) as ApiResponse<T>;
      return response.result;
    } catch (error) {
      return (error as AxiosError & { result?: ApiResult<T> })?.result || { 
        success: false, 
        error: new ApiError(0, { title: 'Unknown Error', status: 0 }) 
      };
    }
  }

  public async post<T = unknown>(url: string, data?: unknown, config?: Record<string, unknown>): Promise<ApiResult<T>> {
    try {
      const response = await this.axiosInstance.post<T>(url, data, config) as ApiResponse<T>;
      return response.result;
    } catch (error) {
      return (error as AxiosError & { result?: ApiResult<T> })?.result || { 
        success: false, 
        error: new ApiError(0, { title: 'Unknown Error', status: 0 }) 
      };
    }
  }

  public async put<T = unknown>(url: string, data?: unknown, config?: Record<string, unknown>): Promise<ApiResult<T>> {
    try {
      const response = await this.axiosInstance.put<T>(url, data, config) as ApiResponse<T>;
      return response.result;
    } catch (error) {
      return (error as AxiosError & { result?: ApiResult<T> })?.result || { 
        success: false, 
        error: new ApiError(0, { title: 'Unknown Error', status: 0 }) 
      };
    }
  }

  public async delete<T = unknown>(url: string, config?: Record<string, unknown>): Promise<ApiResult<T>> {
    try {
      const response = await this.axiosInstance.delete<T>(url, config) as ApiResponse<T>;
      return response.result;
    } catch (error) {
      return (error as AxiosError & { result?: ApiResult<T> })?.result || { 
        success: false, 
        error: new ApiError(0, { title: 'Unknown Error', status: 0 }) 
      };
    }
  }
}

export const apiClient = new ApiClient();

/**
 * Helper function to handle API results in components
 */
export async function handleApiResult<T>(
  resultPromise: Promise<ApiResult<T>>,
  options: {
    onSuccess?: (data: T) => void;
    onError?: (error: ApiError) => void;
    onValidationError?: (errors: Record<string, string[]>) => void;
  } = {}
): Promise<T | undefined> {
  const result = await resultPromise;
  
  if (result.success) {
    options.onSuccess?.(result.data);
    return result.data;
  } else {
    if (result.error.isValidationError && options.onValidationError && result.error.problem.errors) {
      options.onValidationError(result.error.problem.errors);
    } else {
      options.onError?.(result.error);
    }
    return undefined;
  }
}

/**
 * React hook helper for API calls (if using React)
 */
export function useApiResult<T>(result: ApiResult<T> | undefined) {
  if (!result) {
    return {
      data: undefined,
      error: undefined,
      isLoading: true,
      isError: false,
      isSuccess: false,
    };
  }

  return {
    data: result.success ? result.data : undefined,
    error: result.success ? undefined : result.error,
    isLoading: false,
    isError: !result.success,
    isSuccess: result.success,
  };
}

/**
 * Utility to extract validation errors from an ApiError for form handling
 */
export function extractValidationErrors(error: ApiError): Record<string, string> {
  if (!error.isValidationError || !error.problem.errors) {
    return {};
  }

  const formErrors: Record<string, string> = {};
  Object.entries(error.problem.errors).forEach(([field, messages]) => {
    formErrors[field] = messages[0]; // Take the first error message
  });
  
  return formErrors;
}