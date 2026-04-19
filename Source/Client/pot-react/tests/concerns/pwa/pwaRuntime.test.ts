import { beforeEach, describe, expect, test, vi } from 'vitest';

import {
  hasLaterSnoozeExpired,
  isLaterSnoozeActive,
  LATER_SNOOZE_MS,
  pwaRuntimeState,
  UPDATE_CHECK_INTERVAL_MS,
  UPDATE_TOAST_ID,
} from '@/concerns/pwa/pwaRuntime';

describe('pwaRuntime', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    pwaRuntimeState.dismissedWaitingScriptUrl = undefined;
    pwaRuntimeState.dismissedWaitingScriptAt = undefined;
  });

  test('exposes expected runtime constants', () => {
    expect(UPDATE_TOAST_ID).toBe('pwa-update-available');
    expect(UPDATE_CHECK_INTERVAL_MS).toBe(LATER_SNOOZE_MS);
  });

  test('returns false for active snooze when key does not match or timestamp is missing', () => {
    pwaRuntimeState.dismissedWaitingScriptUrl = '/other-sw.js';
    pwaRuntimeState.dismissedWaitingScriptAt = Date.now();

    expect(isLaterSnoozeActive('/sw.js')).toBe(false);

    pwaRuntimeState.dismissedWaitingScriptUrl = '/sw.js';
    pwaRuntimeState.dismissedWaitingScriptAt = undefined;

    expect(isLaterSnoozeActive('/sw.js')).toBe(false);
  });

  test('returns true for active snooze inside timeout window', () => {
    const now = 2_000_000;
    vi.spyOn(Date, 'now').mockReturnValue(now);

    pwaRuntimeState.dismissedWaitingScriptUrl = '/sw.js';
    pwaRuntimeState.dismissedWaitingScriptAt = now - LATER_SNOOZE_MS + 1;

    expect(isLaterSnoozeActive('/sw.js')).toBe(true);
  });

  test('returns false for active snooze when timeout window has elapsed', () => {
    const now = 3_000_000;
    vi.spyOn(Date, 'now').mockReturnValue(now);

    pwaRuntimeState.dismissedWaitingScriptUrl = '/sw.js';
    pwaRuntimeState.dismissedWaitingScriptAt = now - LATER_SNOOZE_MS;

    expect(isLaterSnoozeActive('/sw.js')).toBe(false);
  });

  test('returns false for snooze expiry when no dismissal has been recorded', () => {
    expect(hasLaterSnoozeExpired()).toBe(false);
  });

  test('returns false before snooze expiry and true once threshold is reached', () => {
    const now = 4_000_000;
    vi.spyOn(Date, 'now').mockReturnValue(now);

    pwaRuntimeState.dismissedWaitingScriptUrl = '/sw.js';
    pwaRuntimeState.dismissedWaitingScriptAt = now - LATER_SNOOZE_MS + 1;

    expect(hasLaterSnoozeExpired()).toBe(false);

    pwaRuntimeState.dismissedWaitingScriptAt = now - LATER_SNOOZE_MS;

    expect(hasLaterSnoozeExpired()).toBe(true);
  });
});
