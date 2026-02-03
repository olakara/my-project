import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import authApi from '@/services/api/authApi';
import type { User, LoginRequest, RegisterRequest } from '@/types/auth.types';

interface AuthStore {
  // State
  user: User | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;

  // Actions
  login: (credentials: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  setUser: (user: User | null) => void;
  setAccessToken: (token: string | null) => void;
  clearError: () => void;
  initializeAuth: () => Promise<void>;
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      // Initial state
      user: null,
      accessToken: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,

      // Login action
      login: async (credentials: LoginRequest) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authApi.login(credentials);
          
          const user: User = {
            userId: response.userId,
            email: response.email,
            fullName: response.fullName,
          };

          set({
            user,
            accessToken: response.accessToken,
            isAuthenticated: true,
            isLoading: false,
            error: null,
          });
        } catch (error: any) {
          const errorMessage = error.response?.data?.error || 'Login failed. Please try again.';
          set({
            user: null,
            accessToken: null,
            isAuthenticated: false,
            isLoading: false,
            error: errorMessage,
          });
          throw error;
        }
      },

      // Register action
      register: async (data: RegisterRequest) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authApi.register(data);
          
          const user: User = {
            userId: response.userId,
            email: response.email,
            fullName: response.fullName,
          };

          set({
            user,
            accessToken: response.accessToken,
            isAuthenticated: true,
            isLoading: false,
            error: null,
          });
        } catch (error: any) {
          const errorMessage = error.response?.data?.error || 'Registration failed. Please try again.';
          set({
            user: null,
            accessToken: null,
            isAuthenticated: false,
            isLoading: false,
            error: errorMessage,
          });
          throw error;
        }
      },

      // Logout action
      logout: async () => {
        set({ isLoading: true, error: null });
        try {
          await authApi.logout();
        } catch (error: any) {
          console.error('Logout error:', error);
          // Continue with logout even if API call fails
        } finally {
          set({
            user: null,
            accessToken: null,
            isAuthenticated: false,
            isLoading: false,
            error: null,
          });
        }
      },

      // Set user (used when fetching current user)
      setUser: (user: User | null) => {
        set({
          user,
          isAuthenticated: user !== null,
        });
      },

      // Set access token
      setAccessToken: (token: string | null) => {
        set({
          accessToken: token,
          isAuthenticated: token !== null,
        });
      },

      // Clear error
      clearError: () => {
        set({ error: null });
      },

      // Initialize auth state from storage and verify token
      initializeAuth: async () => {
        const { accessToken } = get();
        
        if (!accessToken) {
          set({ isAuthenticated: false, user: null });
          return;
        }

        set({ isLoading: true });
        try {
          // Verify token is still valid by fetching current user
          const user = await authApi.getCurrentUser();
          set({
            user,
            isAuthenticated: true,
            isLoading: false,
          });
        } catch (error) {
          // Token is invalid or expired
          set({
            user: null,
            accessToken: null,
            isAuthenticated: false,
            isLoading: false,
          });
          localStorage.removeItem('accessToken');
        }
      },
    }),
    {
      name: 'auth-storage', // unique name for localStorage key
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        // Only persist user and accessToken
        user: state.user,
        accessToken: state.accessToken,
      }),
    }
  )
);

export default useAuthStore;
