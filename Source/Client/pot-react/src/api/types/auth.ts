/**
 * JWT authentication token response from the server
 */
type AuthTokens = {
  accessToken: string;
  refreshToken: string;
  refreshTokenExpiryUtc: string;
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
  RefreshTokenRequest,
  TokenProvider,
};
