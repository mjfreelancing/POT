import { faker } from '@faker-js/faker';
import { AxiosError, AxiosHeaders, InternalAxiosRequestConfig } from 'axios';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import {
  addCorrelationId,
  getNetworkError,
  getNetworkErrorMessage,
} from '@/api/apiHelpers';
import { NetworkError } from '@/api/errors/apiErrors';

describe('API helper utilities', () => {
  describe('getNetworkErrorMessage', () => {
    let message = '';

    beforeEach(() => {
      message = faker.lorem.sentence();
    });

    it('should handle connection aborted', () => {
      const error = new AxiosError(message, 'ECONNABORTED');
      const result = getNetworkErrorMessage(error);

      expect(result).toEqual('The connection was aborted');
    });

    it('should handle connection timeout', () => {
      const error = new AxiosError(message, 'ETIMEDOUT');
      const result = getNetworkErrorMessage(error);

      expect(result).toEqual('Request timed out while waiting for the server');
    });

    it('should handle network errors', () => {
      const error = new AxiosError(message, 'ERR_NETWORK');
      const result = getNetworkErrorMessage(error);

      expect(result).toEqual('Unable to connect to the server');
    });

    it('should handle bad requests', () => {
      const error = new AxiosError(message, 'ERR_BAD_REQUEST');
      const result = getNetworkErrorMessage(error);

      expect(result).toEqual('Invalid request');
    });

    it('should handle bad responses', () => {
      const error = new AxiosError(message, 'ERR_BAD_RESPONSE');
      const result = getNetworkErrorMessage(error);

      expect(result).toEqual('Server returned an invalid response');
    });

    it('should handle unsupported operation', () => {
      const error = new AxiosError(message, 'ERR_NOT_SUPPORT');
      const result = getNetworkErrorMessage(error);

      expect(result).toEqual('Operation not supported');
    });

    it('should handle invalid url', () => {
      const error = new AxiosError(message, 'ERR_INVALID_URL');
      const result = getNetworkErrorMessage(error);

      expect(result).toEqual('Invalid server URL');
    });

    it('should handle too many redirects', () => {
      const error = new AxiosError(message, 'ERR_FR_TOO_MANY_REDIRECTS');
      const result = getNetworkErrorMessage(error);

      expect(result).toEqual('Too many redirects');
    });

    it('should handle unknown errors', () => {
      const error = new AxiosError(message, 'UNKNOWN');
      const result = getNetworkErrorMessage(error);

      expect(result).toEqual(`Network error: ${error.message}`);
    });
  });

  describe('getNetworkError', () => {
    let message = '';

    beforeEach(() => {
      message = faker.lorem.sentence();
    });

    it('should handle connection aborted', () => {
      const error = new AxiosError(message, 'ECONNABORTED');
      const result = getNetworkError(error);

      expect(result).toBeInstanceOf(NetworkError);
      expect(result.description).toEqual(
        'The connection was aborted (ECONNABORTED)',
      );
    });

    it('should handle connection timeout', () => {
      const error = new AxiosError(message, 'ETIMEDOUT');
      const result = getNetworkError(error);

      expect(result).toBeInstanceOf(NetworkError);
      expect(result.description).toEqual(
        'Request timed out while waiting for the server (ETIMEDOUT)',
      );
    });

    it('should handle network errors', () => {
      const error = new AxiosError(message, 'ERR_NETWORK');
      const result = getNetworkError(error);

      expect(result).toBeInstanceOf(NetworkError);
      expect(result.description).toEqual(
        'Unable to connect to the server (ERR_NETWORK)',
      );
    });

    it('should handle bad requests', () => {
      const error = new AxiosError(message, 'ERR_BAD_REQUEST');
      const result = getNetworkError(error);

      expect(result).toBeInstanceOf(NetworkError);
      expect(result.description).toEqual('Invalid request (ERR_BAD_REQUEST)');
    });

    it('should handle bad responses', () => {
      const error = new AxiosError(message, 'ERR_BAD_RESPONSE');
      const result = getNetworkError(error);

      expect(result).toBeInstanceOf(NetworkError);
      expect(result.description).toEqual(
        'Server returned an invalid response (ERR_BAD_RESPONSE)',
      );
    });

    it('should handle unsupported operation', () => {
      const error = new AxiosError(message, 'ERR_NOT_SUPPORT');
      const result = getNetworkError(error);

      expect(result).toBeInstanceOf(NetworkError);
      expect(result.description).toEqual(
        'Operation not supported (ERR_NOT_SUPPORT)',
      );
    });

    it('should handle invalid url', () => {
      const error = new AxiosError(message, 'ERR_INVALID_URL');
      const result = getNetworkError(error);

      expect(result).toBeInstanceOf(NetworkError);
      expect(result.description).toEqual(
        'Invalid server URL (ERR_INVALID_URL)',
      );
    });

    it('should handle too many redirects', () => {
      const error = new AxiosError(message, 'ERR_FR_TOO_MANY_REDIRECTS');
      const result = getNetworkError(error);

      expect(result).toBeInstanceOf(NetworkError);
      expect(result.description).toEqual(
        'Too many redirects (ERR_FR_TOO_MANY_REDIRECTS)',
      );
    });

    it('should handle unknown errors', () => {
      const error = new AxiosError(message, 'UNKNOWN');
      const result = getNetworkError(error);

      expect(result).toBeInstanceOf(NetworkError);
      expect(result.description).toEqual(`Network error: ${message} (UNKNOWN)`);
    });

    it('should handle errors without a code', () => {
      const error = new AxiosError(message);
      error.code = undefined;
      const result = getNetworkError(error);

      expect(result).toBeInstanceOf(NetworkError);
      expect(result.description).toEqual(`Network error: ${message} (UNKNOWN)`);
    });

    it('should handle errors with an unkown code', () => {
      const error = new AxiosError(message, faker.string.alphanumeric(8));
      const result = getNetworkError(error);

      expect(result).toBeInstanceOf(NetworkError);
      expect(result.description).toEqual(
        `Network error: ${message} (${error.code})`,
      );
    });
  });

  describe('addCorrelationId', () => {
    const mockUuid = crypto.randomUUID();

    beforeEach(() => {
      vi.spyOn(crypto, 'randomUUID').mockReturnValue(mockUuid);
    });

    afterEach(() => {
      vi.restoreAllMocks();
    });

    it('should add X-Correlation-ID to config without existing headers', () => {
      const config: InternalAxiosRequestConfig = {
        headers: new AxiosHeaders(),
      };
      const result = addCorrelationId(config);

      expect(result.headers).toBeDefined();
      expect(result.headers['X-Correlation-ID']).toEqual(mockUuid);
    });

    it('should add X-Correlation-ID to config with existing headers', () => {
      const headers = new AxiosHeaders();
      headers.set('Content-Type', 'application/json');

      const config: InternalAxiosRequestConfig = {
        headers,
      };
      const result = addCorrelationId(config);

      expect(result.headers['Content-Type']).toEqual('application/json');
      expect(result.headers['X-Correlation-ID']).toEqual(mockUuid);
    });

    it('should use the mocked UUID value', () => {
      const config: InternalAxiosRequestConfig = {
        headers: new AxiosHeaders(),
      };
      const result = addCorrelationId(config);

      expect(crypto.randomUUID).toHaveBeenCalledTimes(1);
      expect(result.headers['X-Correlation-ID']).toEqual(mockUuid);
    });

    it('should return the same config instance', () => {
      const config: InternalAxiosRequestConfig = {
        headers: new AxiosHeaders(),
      };
      const result = addCorrelationId(config);

      expect(result).toBe(config);
    });

    it('should not override existing X-Correlation-ID if present', () => {
      const existingId = faker.string.uuid();
      const headers = new AxiosHeaders();
      headers.set('X-Correlation-ID', existingId);

      const config: InternalAxiosRequestConfig = {
        headers,
      };

      const result = addCorrelationId(config);

      expect(result.headers['X-Correlation-ID']).toEqual(existingId);
      expect(crypto.randomUUID).not.toHaveBeenCalled();
    });
  });
});
