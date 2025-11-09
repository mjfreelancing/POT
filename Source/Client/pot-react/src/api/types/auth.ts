/**
 * JWT authentication token response from the server
 */
type AuthTokens = {
  accessToken: string;
  refreshToken: string;
};

/**
 * Login response status
 */
type LoginStatus = 'Success' | 'Approval';

/**
 * Login response from the server
 */
type LoginResponse = {
  status: LoginStatus;
  accessToken?: string;
  refreshToken?: string;
  message?: string;
};

/**
 * Request body for refreshing tokens
 */
type RefreshTokenRequest = {
  refreshToken: string;
};

/**
 * Token provider interface for authentication operations
 */
type TokenProvider = {
  getAccessToken: () => string | undefined;
  refreshTokens: () => Promise<string>;
  clearTokens: () => void;
};

/**
 * Login request credentials
 */
type LoginCredentials = {
  username: string;
  password: string;
};

export type {
  AuthTokens,
  LoginCredentials,
  LoginResponse,
  LoginStatus,
  RefreshTokenRequest,
  TokenProvider,
};
