import { beforeEach, describe, expect, test, vi } from 'vitest';

import logoutManager from '@/concerns/auth/logoutManager';

describe('logoutManager', () => {
  beforeEach(() => {
    logoutManager.setLogoutCallback(() => undefined);
  });

  test('invokes registered callback when logout is called', () => {
    const logoutCallback = vi.fn();

    logoutManager.setLogoutCallback(logoutCallback);
    logoutManager.logout();

    expect(logoutCallback).toHaveBeenCalledTimes(1);
  });

  test('uses most recently registered callback', () => {
    const previousCallback = vi.fn();
    const currentCallback = vi.fn();

    logoutManager.setLogoutCallback(previousCallback);
    logoutManager.setLogoutCallback(currentCallback);
    logoutManager.logout();

    expect(previousCallback).not.toHaveBeenCalled();
    expect(currentCallback).toHaveBeenCalledTimes(1);
  });

  test('does not throw when callback is a no-op', () => {
    logoutManager.setLogoutCallback(() => undefined);

    expect(() => logoutManager.logout()).not.toThrow();
  });
});
