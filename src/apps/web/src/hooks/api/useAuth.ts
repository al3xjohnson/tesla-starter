import { useApiQuery } from './useApiQuery';
import { useApiMutation } from './useApiMutation';
import { authService } from '@/services/auth.service';
import { useQueryClient } from '@tanstack/react-query';

export const useUsers = (enabled = true) => {
  return useApiQuery({
    queryKey: ['users'],
    queryFn: () => authService.getAllUsers(),
    enabled,
  });
};

export const useMyVehicles = (enabled = true) => {
  return useApiQuery({
    queryKey: ['myVehicles'],
    queryFn: () => authService.getMyVehicles(),
    enabled,
  });
};

export const useSyncVehicles = () => {
  const queryClient = useQueryClient();
  
  return useApiMutation({
    mutationFn: () => authService.syncVehicles(),
    successMessage: (data) => data.message,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['myVehicles'] });
    },
  });
};

export const useUnlinkTeslaAccount = () => {
  const queryClient = useQueryClient();
  
  return useApiMutation({
    mutationFn: () => authService.unlinkTeslaAccount(),
    successMessage: 'Tesla account unlinked successfully',
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['auth'] });
    },
  });
};

export const useRefreshTeslaTokens = () => {
  return useApiMutation({
    mutationFn: () => authService.refreshTeslaTokens(),
    successMessage: 'Tesla tokens refreshed successfully',
  });
};

export const useInitiateTeslaAuth = () => {
  return useApiMutation({
    mutationFn: () => authService.initiateTeslaAuth(),
    showToast: false, // Handle errors manually for this flow
  });
};