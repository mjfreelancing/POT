import { AxiosError, InternalAxiosRequestConfig } from 'axios';

import { NetworkError } from './errors/apiErrors';

/**
 * Gets a readable message for network errors
 */
function getNetworkErrorMessage(error: AxiosError): string {
  switch (error.code) {
    case AxiosError.ECONNABORTED:
      return 'The connection was aborted';

    case AxiosError.ETIMEDOUT:
      return 'Request timed out while waiting for the server';

    case AxiosError.ERR_NETWORK:
      return 'Unable to connect to the server';

    case AxiosError.ERR_BAD_REQUEST:
      return 'Invalid request';

    case AxiosError.ERR_BAD_RESPONSE:
      return 'Server returned an invalid response';

    case AxiosError.ERR_NOT_SUPPORT:
      return 'Operation not supported';

    case AxiosError.ERR_INVALID_URL:
      return 'Invalid server URL';

    case AxiosError.ERR_FR_TOO_MANY_REDIRECTS:
      return 'Too many redirects';

    default:
      return `Network error: ${error.message}`;
  }
}

/**
 * Creates a NetworkError instance from an AxiosError
 */
function getNetworkError(error: AxiosError): NetworkError {
  const message = getNetworkErrorMessage(error);
  return new NetworkError(message);
}

/**
 * Adds correlation ID to request headers
 */
function addCorrelationId(
  config: InternalAxiosRequestConfig,
): InternalAxiosRequestConfig {
  config.headers = config.headers || {};

  // Only set X-Correlation-ID if it doesn't already exist
  if (!config.headers['X-Correlation-ID']) {
    config.headers['X-Correlation-ID'] = crypto.randomUUID();
  }

  return config;
}

export { addCorrelationId, getNetworkError, getNetworkErrorMessage };
