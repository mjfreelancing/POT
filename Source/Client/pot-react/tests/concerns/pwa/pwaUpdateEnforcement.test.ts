import { beforeEach, describe, expect, test, vi } from 'vitest';

import {
  pwaRuntimeState,
  UPDATE_ENFORCEMENT_TICK_MS,
  UPDATE_MAX_DEFER_MS,
  UPDATE_QUIET_PERIOD_MS,
} from '@/concerns/pwa/pwaRuntime';
import {
  startUpdateEnforcement,
  stopUpdateEnforcement,
} from '@/concerns/pwa/pwaUpdateEnforcement';

vi.mock('@/concerns/logging', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
  },
}));

// Activity listeners are attached once for the module lifetime, so resetting this state between
// tests only clears the pending update and its evaluation timer.
const resetRuntimeState = () => {
  pwaRuntimeState.enforcementIntervalId = undefined;
  pwaRuntimeState.pendingUpdateScriptUrl = undefined;
  pwaRuntimeState.pendingUpdateDetectedAt = undefined;
  pwaRuntimeState.lastUserActivityAt = undefined;
};

const simulateUserActivity = () => {
  window.dispatchEvent(new Event('keydown'));
};

// Advances the clock in slices while the user keeps interacting, which is what the quiet window
// reacts to. The slice must stay inside the quiet period so every slice counts as active use.
const advanceWhileUserKeepsInteracting = (totalMs: number) => {
  const sliceMs = 10_000;
  let elapsedMs = 0;

  while (elapsedMs < totalMs) {
    vi.advanceTimersByTime(sliceMs);
    elapsedMs += sliceMs;
    simulateUserActivity();
  }
};

describe('startUpdateEnforcement', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.useFakeTimers();
    resetRuntimeState();
  });

  test('does not apply the update before the quiet period elapses', () => {
    const applyUpdate = vi.fn();

    startUpdateEnforcement('/sw.js', applyUpdate);

    vi.advanceTimersByTime(UPDATE_QUIET_PERIOD_MS - UPDATE_ENFORCEMENT_TICK_MS);

    expect(applyUpdate).not.toHaveBeenCalled();
  });

  test('applies the update once the user has been quiet for the quiet period', () => {
    const applyUpdate = vi.fn();

    startUpdateEnforcement('/sw.js', applyUpdate);

    vi.advanceTimersByTime(UPDATE_QUIET_PERIOD_MS);

    expect(applyUpdate).toHaveBeenCalledTimes(1);
    expect(pwaRuntimeState.pendingUpdateDetectedAt).toBeUndefined();
  });

  test('extends the quiet window while the user keeps interacting', () => {
    const applyUpdate = vi.fn();

    startUpdateEnforcement('/sw.js', applyUpdate);

    advanceWhileUserKeepsInteracting(UPDATE_QUIET_PERIOD_MS * 3);

    expect(applyUpdate).not.toHaveBeenCalled();
  });

  test('applies the update at the maximum deferral window even while the user stays active', () => {
    const applyUpdate = vi.fn();

    startUpdateEnforcement('/sw.js', applyUpdate);

    advanceWhileUserKeepsInteracting(UPDATE_MAX_DEFER_MS + 10_000);

    expect(applyUpdate).toHaveBeenCalledTimes(1);
  });

  test('applies the update only once per detected update', () => {
    const applyUpdate = vi.fn();

    startUpdateEnforcement('/sw.js', applyUpdate);

    vi.advanceTimersByTime(
      UPDATE_QUIET_PERIOD_MS + UPDATE_ENFORCEMENT_TICK_MS * 5,
    );

    expect(applyUpdate).toHaveBeenCalledTimes(1);
  });

  test('replaces the pending update when enforcement starts again', () => {
    const firstApplyUpdate = vi.fn();
    const secondApplyUpdate = vi.fn();

    startUpdateEnforcement('/sw-a.js', firstApplyUpdate);
    startUpdateEnforcement('/sw-b.js', secondApplyUpdate);

    vi.advanceTimersByTime(UPDATE_QUIET_PERIOD_MS);

    expect(firstApplyUpdate).not.toHaveBeenCalled();
    expect(secondApplyUpdate).toHaveBeenCalledTimes(1);
    expect(pwaRuntimeState.pendingUpdateScriptUrl).toBeUndefined();
  });

  test('stops applying the update when enforcement is stopped', () => {
    const applyUpdate = vi.fn();

    startUpdateEnforcement('/sw.js', applyUpdate);
    stopUpdateEnforcement();

    vi.advanceTimersByTime(UPDATE_MAX_DEFER_MS);

    expect(applyUpdate).not.toHaveBeenCalled();
    expect(pwaRuntimeState.pendingUpdateScriptUrl).toBeUndefined();
  });
});
