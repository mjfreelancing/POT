import { formatDateTime } from './dateUtils';
import { logger } from './logging';

const JWT_LOGGER = 'JWT';

import { jwtDecode } from 'jwt-decode';

/**
 * Decodes a JWT token without verification
 */
function decodeJwt(token: string) {
  try {
    return jwtDecode(token);
  } catch (error) {
    logger.error(JWT_LOGGER, 'Failed to decode JWT token', error);
    return null;
  }
}

/**
 * Gets the expiration time of a JWT token in milliseconds
 */
function getTokenExpirationTime(token: string): number | null {
  const decoded = decodeJwt(token);
  if (!decoded || !decoded.exp) {
    return null;
  }

  const expirationMs = decoded.exp * 1000; // Convert from seconds to milliseconds
  const expirationDate = new Date(expirationMs);

  logger.info(
    JWT_LOGGER,
    'JWT token expires at:',
    formatDateTime(expirationDate, 'en-AU'),
  );

  return expirationMs;
}

/**
 * Checks if a token is nearing expiration (within the given buffer time)
 * @param token The JWT token to check
 * @param bufferMs Time in milliseconds before expiration to consider the token as expiring
 * @returns true if the token is expiring soon
 */
function isTokenExpiringSoon(token: string, bufferMs = 5 * 60 * 1000): boolean {
  const expirationTime = getTokenExpirationTime(token);
  if (!expirationTime) {
    return false;
  }

  const currentTime = Date.now();
  const timeUntilExpiry = expirationTime - currentTime;
  const isExpiring = timeUntilExpiry <= bufferMs;

  if (isExpiring) {
    logger.info(
      JWT_LOGGER,
      `Token expiring in ${Math.round(timeUntilExpiry / 1000)} seconds`,
    );
  }

  return isExpiring;
}

/**
 * Calculate when to refresh the token based on its expiration time
 * @param token The JWT token
 * @param bufferMs Time in milliseconds before expiration to refresh the token
 * @returns Time in milliseconds until the token should be refreshed, or null if token is invalid
 */
function calculateRefreshTime(
  token: string,
  bufferMs = 5 * 60 * 1000,
): number | null {
  const expirationTime = getTokenExpirationTime(token);

  if (!expirationTime) {
    return null;
  }

  const refreshTime = expirationTime - bufferMs;
  const now = Date.now();
  const timeUntilRefresh = Math.max(0, refreshTime - now);

  logger.info(
    JWT_LOGGER,
    `Token refresh scheduled for ${formatDateTime(
      new Date(now + timeUntilRefresh),
    )}`,
  );

  return timeUntilRefresh;
}

export {
  calculateRefreshTime,
  decodeJwt,
  getTokenExpirationTime,
  isTokenExpiringSoon,
};
