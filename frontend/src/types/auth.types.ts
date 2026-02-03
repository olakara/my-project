// Authentication-related TypeScript types and interfaces

export interface User {
  userId: string;
  email: string;
  fullName: string;
  firstName?: string;
  lastName?: string;
}

export interface AuthToken {
  accessToken: string;
  expiresIn: number; // seconds
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  userId: string;
  email: string;
  fullName: string;
  accessToken: string;
  expiresIn: number;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface RegisterResponse {
  userId: string;
  email: string;
  fullName: string;
  accessToken: string;
  expiresIn: number;
}

export interface RefreshTokenResponse {
  accessToken: string;
  expiresIn: number;
}

export interface AuthState {
  user: User | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

export interface AuthError {
  error: string;
  correlationId?: string;
  lockedUntil?: string;
}
