/**
 * JWT authentication token response from the server
 * Note: refreshToken is now stored in HTTP-only cookie, not returned in response
 */
type AuthTokens = {
  accessToken: string;
};

/**
 * Login response status
 */
type LoginStatus = 'Success' | 'Approval';

/**
 * Login response from the server
 * Note: refreshToken is set as HTTP-only cookie, not in response body
 * - Success status: accessToken is always present, message is undefined
 * - Approval status: message is always present, accessToken is undefined
 */
type LoginResponse =
  | {
      status: 'Success';
      accessToken: string;
      message?: undefined;
    }
  | {
      status: 'Approval';
      accessToken?: undefined;
      message: string;
    };

/**
 * Token provider interface for authentication operations
 */
type TokenProvider = {
  getAccessToken: () => string | undefined;
  refreshTokens: () => Promise<string>;
  clearTokens: () => void;
  setAccessToken: (token: string | undefined) => void;
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
  TokenProvider,
};
