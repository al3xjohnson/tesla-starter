import { useQuery, UseQueryOptions, UseQueryResult } from '@tanstack/react-query';
import { ApiResult, ApiError } from '@/services/api-client';

export interface UseApiQueryOptions<TData> extends Omit<UseQueryOptions<TData, ApiError>, 'queryFn'> {
  queryFn: () => Promise<ApiResult<TData>>;
}

export function useApiQuery<TData>(
  options: UseApiQueryOptions<TData>
): UseQueryResult<TData, ApiError> {
  return useQuery({
    ...options,
    queryFn: async () => {
      const result = await options.queryFn();
      if (result.success) {
        return result.data;
      }
      throw result.error;
    },
  });
}