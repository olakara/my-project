import apiClient from './apiClient';
import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
  RefreshTokenResponse,
  User,
} from '@/types/auth.types';

/**
 * Authentication API client
 * Handles user registration, login, logout, and token refresh
 */

export const authApi = {
  /**
   * Register a new user account
   * @param data - Registration details (email, password, firstName, lastName)
   * @returns User profile and access token
   */
  async register(data: RegisterRequest): Promise<RegisterResponse> {
    const response = await apiClient.post<RegisterResponse>('/auth/register', data);
    
    // Store access token in localStorage
    if (response.data.accessToken) {
      localStorage.setItem('accessToken', response.data.accessToken);
    }
    
    return response.data;
  },

  /**
   * Authenticate user and receive JWT tokens
   * @param data - Login credentials (email, password)
   * @returns User profile and access token
   */
  async login(data: LoginRequest): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>('/auth/login', data);
    
    // Store access token in localStorage
    // Refresh token is automatically stored in HTTP-only cookie
    if (response.data.accessToken) {
      localStorage.setItem('accessToken', response.data.accessToken);
    }
    
    return response.data;
  },

  /**
   * Logout user and revoke refresh token
   * Clears local storage and removes HTTP-only cookie
   */
  async logout(): Promise<void> {
    try {
      await apiClient.post('/auth/logout');
    } finally {
      // Clear local storage even if API call fails
      localStorage.removeItem('accessToken');
      localStorage.removeItem('user');
    }
  },

  /**
   * Refresh access token using refresh token from cookie
   * @returns New access token
   */
  async refreshToken(): Promise<RefreshTokenResponse> {
    const response = await apiClient.post<RefreshTokenResponse>('/auth/refresh');
    
    // Store new access token
    if (response.data.accessToken) {
      localStorage.setItem('accessToken', response.data.accessToken);
    }
    
    return response.data;
  },

  /**
   * Get current authenticated user profile
   * @returns User profile
   */
  async getCurrentUser(): Promise<User> {
    const response = await apiClient.get<User>('/auth/me');
    return response.data;
  },
};

export default authApi;
