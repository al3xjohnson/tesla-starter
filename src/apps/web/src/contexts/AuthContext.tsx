import React, { createContext, useContext, useEffect, useState } from 'react';
import { useDescope, useSession, useUser, getSessionToken } from '@descope/react-sdk';
import { AuthState, UserDto } from '@teslastarter/api-contracts';
import { authService } from '@/services/auth.service';
import { apiClient } from '@/services/api-client';

interface AuthContextValue extends AuthState {
  login: () => void;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: React.ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const { isAuthenticated, isSessionLoading } = useSession();
  const { isUserLoading } = useUser();
  const { logout: descopeLogout } = useDescope();
  const [appUser, setAppUser] = useState<UserDto | null>(null);
  const [isLoadingUser, setIsLoadingUser] = useState(false);

  // Sync user data when authentication changes
  useEffect(() => {
    const syncUser = async () => {
      if (isAuthenticated && !isSessionLoading && !isUserLoading) {
        setIsLoadingUser(true);
        try {

          // Fetch or create user in our backend
          const result = await authService.getCurrentUser();
          if (result.success) {
            setAppUser(result.data);
          }
        } catch (error) {
          console.error('Failed to fetch user:', error);
        } finally {
          setIsLoadingUser(false);
        }
      } else if (!isAuthenticated && !isSessionLoading) {
        setAppUser(null);
        apiClient.clearAuthToken();
      }
    };

    syncUser();
  }, [isAuthenticated, isSessionLoading, isUserLoading]);

  const login = () => {
    // Login is handled by Descope components
    // This is just a placeholder for the interface
  };

  const logout = async () => {
    try {
      await descopeLogout();
      setAppUser(null);
      apiClient.clearAuthToken();
    } catch (error) {
      console.error('Failed to logout:', error);
    }
  };

  const refreshUser = async () => {
    if (isAuthenticated) {
      setIsLoadingUser(true);
      try {
        const result = await authService.getCurrentUser();
        if (result.success) {
          setAppUser(result.data);
        }
      } catch (error) {
        console.error('Failed to refresh user:', error);
      } finally {
        setIsLoadingUser(false);
      }
    }
  };

  const authState: AuthContextValue = {
    isAuthenticated,
    isLoading: isSessionLoading || isUserLoading || isLoadingUser,
    user: appUser,
    sessionToken: getSessionToken(),
    login,
    logout,
    refreshUser,
  };

  return <AuthContext.Provider value={authState}>{children}</AuthContext.Provider>;
};

