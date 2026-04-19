import { beforeEach, describe, expect, test, vi } from 'vitest';

import { logger } from '@/concerns/logging/logger';

describe('logger', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.spyOn(console, 'info').mockImplementation(() => undefined);
    vi.spyOn(console, 'warn').mockImplementation(() => undefined);
    vi.spyOn(console, 'error').mockImplementation(() => undefined);
  });

  test('formats info and warn messages with component prefix', () => {
    logger.info('PWA', 'Offline cache is ready');
    logger.warn('Auth', 'Token is near expiry');

    expect(console.info).toHaveBeenCalledWith('[PWA] Offline cache is ready');
    expect(console.warn).toHaveBeenCalledWith('[Auth] Token is near expiry');
  });

  test('passes through optional rest arguments for info and warn', () => {
    const metadata = { source: 'manual-check' };

    logger.info('PWA', 'Prompt requested', metadata, 7);
    logger.warn('PWA', 'Prompt deferred', metadata, 9);

    expect(console.info).toHaveBeenCalledWith(
      '[PWA] Prompt requested',
      metadata,
      7,
    );

    expect(console.warn).toHaveBeenCalledWith(
      '[PWA] Prompt deferred',
      metadata,
      9,
    );
  });

  test('logs provided error object and falls back to empty string when absent', () => {
    const registrationError = new Error('registration failed');

    logger.error(
      'PWA',
      'Service worker registration failed',
      registrationError,
    );
    logger.error('PWA', 'Service worker registration failed');

    expect(console.error).toHaveBeenNthCalledWith(
      1,
      '[PWA] Service worker registration failed',
      registrationError,
    );

    expect(console.error).toHaveBeenNthCalledWith(
      2,
      '[PWA] Service worker registration failed',
      '',
    );
  });
});
