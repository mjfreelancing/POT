import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

import { logger } from '@/concerns';
import {
  calculateRefreshTime,
  decodeJwt,
  getTokenExpirationTime,
  isTokenExpiringSoon,
} from '@/lib/jwt';
import { jwtDecode } from 'jwt-decode';

vi.mock('jwt-decode', () => ({
  jwtDecode: vi.fn(),
}));

vi.mock('@/concerns', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
  },
}));

describe('jwt utils', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  test('decodeJwt returns decoded payload', () => {
    const decodedPayload = { sub: 'user-1', exp: 2000 };
    vi.mocked(jwtDecode).mockReturnValue(decodedPayload);

    const result = decodeJwt('valid-token');

    expect(result).toEqual(decodedPayload);
    expect(jwtDecode).toHaveBeenCalledWith('valid-token');
    expect(logger.error).not.toHaveBeenCalled();
  });

  test('decodeJwt returns null and logs when decode throws', () => {
    const decodeError = new Error('invalid token');
    vi.mocked(jwtDecode).mockImplementation(() => {
      throw decodeError;
    });

    const result = decodeJwt('invalid-token');

    expect(result).toBeNull();
    expect(logger.error).toHaveBeenCalledWith(
      'JWT',
      'Failed to decode JWT token',
      decodeError,
    );
  });

  test('getTokenExpirationTime returns milliseconds from exp claim', () => {
    vi.mocked(jwtDecode).mockReturnValue({ exp: 1700 });

    const expirationTime = getTokenExpirationTime('valid-token');

    expect(expirationTime).toBe(1700 * 1000);
    expect(logger.info).toHaveBeenCalled();
  });

  test('getTokenExpirationTime returns null when exp is missing', () => {
    vi.mocked(jwtDecode).mockReturnValue({ sub: 'user-1' });

    const expirationTime = getTokenExpirationTime('token-without-exp');

    expect(expirationTime).toBeNull();
  });

  test('isTokenExpiringSoon returns true when within buffer', () => {
    vi.mocked(jwtDecode).mockReturnValue({ exp: 20 });
    vi.spyOn(Date, 'now').mockReturnValue(10_000);

    const isExpiringSoon = isTokenExpiringSoon('valid-token', 15_000);

    expect(isExpiringSoon).toBe(true);
    expect(logger.info).toHaveBeenCalled();
  });

  test('isTokenExpiringSoon returns false when outside buffer', () => {
    vi.mocked(jwtDecode).mockReturnValue({ exp: 50 });
    vi.spyOn(Date, 'now').mockReturnValue(10_000);

    const isExpiringSoon = isTokenExpiringSoon('valid-token', 20_000);

    expect(isExpiringSoon).toBe(false);
  });

  test('isTokenExpiringSoon returns false for invalid token', () => {
    vi.mocked(jwtDecode).mockImplementation(() => {
      throw new Error('decode failed');
    });

    const isExpiringSoon = isTokenExpiringSoon('invalid-token', 20_000);

    expect(isExpiringSoon).toBe(false);
  });

  test('calculateRefreshTime returns null for token without expiration', () => {
    vi.mocked(jwtDecode).mockReturnValue({ sub: 'user-1' });

    const refreshTime = calculateRefreshTime('token-without-exp', 5_000);

    expect(refreshTime).toBeNull();
  });

  test('calculateRefreshTime returns milliseconds until refresh target', () => {
    vi.mocked(jwtDecode).mockReturnValue({ exp: 40 });
    vi.spyOn(Date, 'now').mockReturnValue(10_000);

    const refreshTime = calculateRefreshTime('valid-token', 5_000);

    expect(refreshTime).toBe(25_000);
  });

  test('calculateRefreshTime returns zero when refresh time is in the past', () => {
    vi.mocked(jwtDecode).mockReturnValue({ exp: 12 });
    vi.spyOn(Date, 'now').mockReturnValue(10_000);

    const refreshTime = calculateRefreshTime('valid-token', 5_000);

    expect(refreshTime).toBe(0);
  });
});
