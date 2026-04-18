import type { MouseEvent } from 'react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import {
  LATER_SNOOZE_MS,
  pwaRuntimeState,
  UPDATE_TOAST_ID,
} from '@/concerns/pwa/pwaRuntime';
import { showUpdatePromptIfNeeded } from '@/concerns/pwa/pwaUpdatePrompt';
import { getWaitingServiceWorkerScriptUrl } from '@/concerns/pwa/serviceWorkerRegistration';
import type { ExternalToast } from 'sonner';
import { toast } from 'sonner';

vi.mock('@/concerns/logging', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
  },
}));

vi.mock('@/concerns/pwa/serviceWorkerRegistration', () => ({
  getWaitingServiceWorkerScriptUrl: vi.fn(),
}));

vi.mock('sonner', () => {
  const toastMock = Object.assign(vi.fn(), {
    dismiss: vi.fn(),
  });

  return {
    toast: toastMock,
  };
});

const flushPromises = async () => {
  await Promise.resolve();
  await Promise.resolve();
};

describe('showUpdatePromptIfNeeded', () => {
  let visibilityState: DocumentVisibilityState;

  beforeEach(() => {
    vi.clearAllMocks();
    vi.useFakeTimers();

    pwaRuntimeState.updateCheckIntervalId = undefined;
    pwaRuntimeState.laterSnoozeTimeoutId = undefined;
    pwaRuntimeState.updateCheckListenersAttached = false;
    pwaRuntimeState.registeredServiceWorkerUrl = undefined;
    pwaRuntimeState.latestServiceWorkerRegistration = undefined;
    pwaRuntimeState.refreshInProgress = false;
    pwaRuntimeState.promptedWaitingScriptUrl = undefined;
    pwaRuntimeState.dismissedWaitingScriptUrl = undefined;
    pwaRuntimeState.dismissedWaitingScriptAt = undefined;

    visibilityState = 'visible';

    Object.defineProperty(document, 'visibilityState', {
      configurable: true,
      get: () => visibilityState,
    });
  });

  test('does not show prompt when there is no waiting worker and prompt is not forced', async () => {
    pwaRuntimeState.promptedWaitingScriptUrl = '/old-sw.js';

    vi.mocked(getWaitingServiceWorkerScriptUrl).mockResolvedValue(undefined);

    const updateServiceWorker = vi.fn().mockResolvedValue(undefined);

    await showUpdatePromptIfNeeded('manual-check', updateServiceWorker);

    expect(toast).not.toHaveBeenCalled();
    expect(pwaRuntimeState.promptedWaitingScriptUrl).toBeUndefined();
  });

  test('shows prompt when waiting worker exists and deduplicates repeat calls', async () => {
    vi.mocked(getWaitingServiceWorkerScriptUrl).mockResolvedValue('/sw.js');

    const updateServiceWorker = vi.fn().mockResolvedValue(undefined);

    await showUpdatePromptIfNeeded('onNeedRefresh-event', updateServiceWorker);

    expect(toast).toHaveBeenCalledTimes(1);
    expect(pwaRuntimeState.promptedWaitingScriptUrl).toBe('/sw.js');

    await showUpdatePromptIfNeeded('onNeedRefresh-event', updateServiceWorker);

    expect(toast).toHaveBeenCalledTimes(1);
  });

  test('later action stores snooze state and re-prompts after snooze expires when visible', async () => {
    vi.mocked(getWaitingServiceWorkerScriptUrl).mockResolvedValue(undefined);

    const updateServiceWorker = vi.fn().mockResolvedValue(undefined);

    pwaRuntimeState.dismissedWaitingScriptUrl = '/sw.js';
    pwaRuntimeState.dismissedWaitingScriptAt = Date.now() - LATER_SNOOZE_MS - 1;

    await showUpdatePromptIfNeeded(
      'force-deferred-check',
      updateServiceWorker,
      true,
    );

    expect(toast).toHaveBeenCalledTimes(1);

    const toastOptions = vi.mocked(toast).mock.calls[0]?.[1] as
      | ExternalToast
      | undefined;

    const cancelAction = toastOptions?.cancel;

    if (
      cancelAction &&
      typeof cancelAction === 'object' &&
      'onClick' in cancelAction
    ) {
      cancelAction.onClick?.({} as MouseEvent<HTMLButtonElement>);
    }

    expect(pwaRuntimeState.dismissedWaitingScriptUrl).toBe('/sw.js');
    expect(pwaRuntimeState.dismissedWaitingScriptAt).toBeTypeOf('number');
    expect(pwaRuntimeState.promptedWaitingScriptUrl).toBeUndefined();
    expect(vi.mocked(toast).dismiss).toHaveBeenCalledWith(UPDATE_TOAST_ID);

    vi.advanceTimersByTime(LATER_SNOOZE_MS);
    await flushPromises();

    expect(toast).toHaveBeenCalledTimes(2);
  });

  test('does not force prompt during active snooze for same worker key', async () => {
    vi.mocked(getWaitingServiceWorkerScriptUrl).mockResolvedValue(undefined);

    pwaRuntimeState.dismissedWaitingScriptUrl = '/sw.js';
    pwaRuntimeState.dismissedWaitingScriptAt = Date.now();

    const updateServiceWorker = vi.fn().mockResolvedValue(undefined);

    await showUpdatePromptIfNeeded(
      'force-deferred-check',
      updateServiceWorker,
      true,
    );

    expect(toast).not.toHaveBeenCalled();
  });
});
