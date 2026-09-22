import { beforeEach, describe, expect, test, vi } from 'vitest';

import {
  isPendingUpdateReadyToApply,
  pwaRuntimeState,
  UPDATE_CHECK_INTERVAL_MS,
  UPDATE_ENFORCEMENT_TICK_MS,
  UPDATE_MAX_DEFER_MS,
  UPDATE_QUIET_PERIOD_MS,
  UPDATE_TOAST_ID,
} from '@/concerns/pwa/pwaRuntime';

describe('pwaRuntime', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    pwaRuntimeState.pendingUpdateScriptUrl = undefined;
    pwaRuntimeState.pendingUpdateDetectedAt = undefined;
    pwaRuntimeState.lastUserActivityAt = undefined;
  });

  test('exposes the stable update toast id and check interval', () => {
    expect(UPDATE_TOAST_ID).toBe('pwa-update-available');
    expect(UPDATE_CHECK_INTERVAL_MS).toBe(30 * 60 * 1000);
  });

  test('keeps the quiet window and evaluation tick inside the maximum deferral window', () => {
    expect(UPDATE_ENFORCEMENT_TICK_MS).toBeLessThan(UPDATE_QUIET_PERIOD_MS);
    expect(UPDATE_QUIET_PERIOD_MS).toBeLessThan(UPDATE_MAX_DEFER_MS);
  });

  test('is not ready to apply while the user is still active inside the quiet window', () => {
    const now = 2_000_000;

    const isReadyToApply = isPendingUpdateReadyToApply({
      detectedAt: now - 1_000,
      lastUserActivityAt: now - 1_000,
      now,
    });

    expect(isReadyToApply).toBe(false);
  });

  test('is ready to apply once the user has been quiet for the quiet window', () => {
    const now = 3_000_000;

    const isReadyToApply = isPendingUpdateReadyToApply({
      detectedAt: now - UPDATE_QUIET_PERIOD_MS,
      lastUserActivityAt: now - UPDATE_QUIET_PERIOD_MS,
      now,
    });

    expect(isReadyToApply).toBe(true);
  });

  test('is ready to apply at the maximum deferral window even while the user stays active', () => {
    const now = 4_000_000;

    const isReadyToApply = isPendingUpdateReadyToApply({
      detectedAt: now - UPDATE_MAX_DEFER_MS,
      lastUserActivityAt: now,
      now,
    });

    expect(isReadyToApply).toBe(true);
  });
});
