import type { InternalAxiosRequestConfig } from 'axios';
import { AxiosError } from 'axios';

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

// A problem body is JSON. A reverse proxy answers a failure with an HTML or plain-text page, and a
// request that asked for a blob brings its error body back as one, so the content type decides
// whether a blob is worth reading at all.
const jsonContentTypePattern = /^application\/(?:[\w.-]+\+)?json\b/i;

const isJsonContentType = (contentType: string): boolean => {
  return contentType === '' || jsonContentTypePattern.test(contentType);
};

const readBodyText = async (data: unknown): Promise<string | undefined> => {
  if (typeof data === 'string') {
    return data;
  }

  if (data instanceof Blob) {
    return isJsonContentType(data.type) ? await data.text() : undefined;
  }

  return undefined;
};

/**
 * Reads an error body into the JSON value it carries
 *
 * Returns undefined when there is nothing to read: an empty body, a page written by a reverse
 * proxy, or a blob whose content type says it is not JSON.
 */
async function readJsonBody(data: unknown): Promise<unknown> {
  if (typeof data === 'object' && data !== null && !(data instanceof Blob)) {
    // axios has already parsed a JSON body.
    return data;
  }

  const text = await readBodyText(data);

  if (!text) {
    return undefined;
  }

  try {
    return JSON.parse(text) as unknown;
  } catch {
    return undefined;
  }
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

export {
  addCorrelationId,
  getNetworkError,
  getNetworkErrorMessage,
  readJsonBody,
};
