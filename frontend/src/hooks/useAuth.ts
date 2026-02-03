import { useEffect } from 'react';
import { useAuthStore } from '@/store/authStore';
import type { LoginRequest, RegisterRequest } from '@/types/auth.types';

/**
 * Custom hook for authentication
 * Provides auth state and actions to components
 */
export const useAuth = () => {
  const {
    user,
    accessToken,
    isAuthenticated,
    isLoading,
    error,
    login,
    register,
    logout,
    clearError,
    initializeAuth,
  } = useAuthStore();

  // Initialize auth state on mount
  useEffect(() => {
    initializeAuth();
  }, [initializeAuth]);

  return {
    // State
    user,
    accessToken,
    isAuthenticated,
    isLoading,
    error,

    // Actions
    login: async (credentials: LoginRequest) => {
      await login(credentials);
    },
    
    register: async (data: RegisterRequest) => {
      await register(data);
    },
    
    logout: async () => {
      await logout();
    },
    
    clearError,
  };
};

export default useAuth;
