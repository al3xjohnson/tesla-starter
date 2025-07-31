import { useMutation, UseMutationOptions, UseMutationResult } from '@tanstack/react-query';
import { ApiResult, ApiError } from '@/services/api-client';
import { useToast } from '@/contexts/ToastContext';

export interface UseApiMutationOptions<TData, TVariables = void>
  extends Omit<UseMutationOptions<TData, ApiError, TVariables>, 'mutationFn'> {
  mutationFn: (variables: TVariables) => Promise<ApiResult<TData>>;
  successMessage?: string | ((data: TData) => string);
  errorMessage?: string | ((error: ApiError) => string);
  showToast?: boolean;
}

export function useApiMutation<TData, TVariables = void>(
  options: UseApiMutationOptions<TData, TVariables>
): UseMutationResult<TData, ApiError, TVariables> {
  const { showError, showSuccess } = useToast();
  const { 
    successMessage, 
    errorMessage, 
    showToast = true,
    onSuccess,
    onError,
    ...mutationOptions 
  } = options;

  return useMutation({
    ...mutationOptions,
    mutationFn: async (variables: TVariables) => {
      const result = await options.mutationFn(variables);
      if (result.success) {
        return result.data;
      }
      throw result.error;
    },
    onSuccess: (data, variables, context) => {
      if (showToast && successMessage) {
        const message = typeof successMessage === 'function' ? successMessage(data) : successMessage;
        showSuccess(message);
      }
      onSuccess?.(data, variables, context);
    },
    onError: (error, variables, context) => {
      if (showToast) {
        const message = errorMessage 
          ? (typeof errorMessage === 'function' ? errorMessage(error) : errorMessage)
          : error.problem.detail || error.problem.title;
        showError(message);
      }
      onError?.(error, variables, context);
    },
  });
}