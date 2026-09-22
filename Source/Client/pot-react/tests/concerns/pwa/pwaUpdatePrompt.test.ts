import type { MouseEvent } from 'react';
import type { Action, ExternalToast } from 'sonner';
import { toast } from 'sonner';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import {
  pwaRuntimeState,
  REFRESH_FALLBACK_TIMEOUT_MS,
  UPDATE_TOAST_ID,
} from '@/concerns/pwa/pwaRuntime';
import { startUpdateEnforcement } from '@/concerns/pwa/pwaUpdateEnforcement';
import { showUpdatePromptIfNeeded } from '@/concerns/pwa/pwaUpdatePrompt';
import { getWaitingServiceWorkerScriptUrl } from '@/concerns/pwa/serviceWorkerRegistration';

vi.mock('@/concerns/logging', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
  },
}));

vi.mock('@/concerns/pwa/serviceWorkerRegistration', () => ({
  getWaitingServiceWorkerScriptUrl: vi.fn(),
}));

vi.mock('@/concerns/pwa/pwaUpdateEnforcement', () => ({
  startUpdateEnforcement: vi.fn(),
}));

vi.mock('sonner', () => {
  const toastMock = Object.assign(vi.fn(), {
    dismiss: vi.fn(),
  });

  return {
    toast: toastMock,
  };
});

const resetRuntimeState = () => {
  pwaRuntimeState.updateCheckIntervalId = undefined;
  pwaRuntimeState.enforcementIntervalId = undefined;
  pwaRuntimeState.updateCheckListenersAttached = false;
  pwaRuntimeState.registeredServiceWorkerUrl = undefined;
  pwaRuntimeState.latestServiceWorkerRegistration = undefined;
  pwaRuntimeState.refreshInProgress = false;
  pwaRuntimeState.promptedWaitingScriptUrl = undefined;
  pwaRuntimeState.pendingUpdateScriptUrl = undefined;
  pwaRuntimeState.pendingUpdateDetectedAt = undefined;
  pwaRuntimeState.lastUserActivityAt = undefined;
};

// jsdom does not implement the service worker API used by the activation path.
const stubServiceWorkerNavigator = () => {
  Object.defineProperty(navigator, 'serviceWorker', {
    configurable: true,
    value: new EventTarget(),
  });
};

const locationReload = vi.fn();

// jsdom marks location.reload as non-configurable, so the whole location is replaced instead.
const stubWindowLocation = () => {
  Object.defineProperty(window, 'location', {
    configurable: true,
    value: { reload: locationReload },
  });
};

const getToastOptions = () =>
  vi.mocked(toast).mock.calls[0]?.[1] as ExternalToast | undefined;

const getToastAction = () => getToastOptions()?.action as Action | undefined;

const flushPromises = async () => {
  await Promise.resolve();
  await Promise.resolve();
};

describe('showUpdatePromptIfNeeded', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Fake timers keep the reload fallback dormant unless a test advances them on purpose.
    vi.useFakeTimers();
    resetRuntimeState();
    stubServiceWorkerNavigator();
    stubWindowLocation();
  });

  test('does not show prompt when there is no waiting worker', async () => {
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

  test('does not show prompt while an update is already being applied', async () => {
    pwaRuntimeState.refreshInProgress = true;

    vi.mocked(getWaitingServiceWorkerScriptUrl).mockResolvedValue('/sw.js');

    const updateServiceWorker = vi.fn().mockResolvedValue(undefined);

    await showUpdatePromptIfNeeded('onNeedRefresh-event', updateServiceWorker);

    expect(toast).not.toHaveBeenCalled();
  });

  test('shows a persistent prompt that can only be applied, not deferred', async () => {
    vi.mocked(getWaitingServiceWorkerScriptUrl).mockResolvedValue('/sw.js');

    const updateServiceWorker = vi.fn().mockResolvedValue(undefined);

    await showUpdatePromptIfNeeded('onNeedRefresh-event', updateServiceWorker);

    const toastOptions = getToastOptions();

    expect(toastOptions?.id).toBe(UPDATE_TOAST_ID);
    expect(toastOptions?.duration).toBe(Infinity);
    expect(toastOptions?.dismissible).toBe(false);
    expect(getToastAction()?.label).toBe('Update now');
    expect(toastOptions?.cancel).toBeUndefined();
  });

  test('starts automatic enforcement for the waiting worker', async () => {
    vi.mocked(getWaitingServiceWorkerScriptUrl).mockResolvedValue('/sw.js');

    const updateServiceWorker = vi.fn().mockResolvedValue(undefined);

    await showUpdatePromptIfNeeded('onNeedRefresh-event', updateServiceWorker);

    expect(startUpdateEnforcement).toHaveBeenCalledWith(
      '/sw.js',
      expect.any(Function),
    );
  });

  test('applies the update when the refresh action is invoked', async () => {
    vi.mocked(getWaitingServiceWorkerScriptUrl).mockResolvedValue('/sw.js');

    const updateServiceWorker = vi.fn().mockResolvedValue(undefined);

    await showUpdatePromptIfNeeded('onNeedRefresh-event', updateServiceWorker);

    getToastAction()?.onClick({} as MouseEvent<HTMLButtonElement>);

    expect(updateServiceWorker).toHaveBeenCalledWith(true);
  });

  test('applies the update when automatic enforcement fires', async () => {
    vi.mocked(getWaitingServiceWorkerScriptUrl).mockResolvedValue('/sw.js');

    const updateServiceWorker = vi.fn().mockResolvedValue(undefined);

    await showUpdatePromptIfNeeded('onNeedRefresh-event', updateServiceWorker);

    const applyUpdate = vi.mocked(startUpdateEnforcement).mock.calls[0]?.[1];

    applyUpdate?.();

    expect(updateServiceWorker).toHaveBeenCalledWith(true);
  });

  test('forces a reload when activation does not hand off control in time', async () => {
    vi.mocked(getWaitingServiceWorkerScriptUrl).mockResolvedValue('/sw.js');

    const updateServiceWorker = vi.fn().mockResolvedValue(undefined);

    await showUpdatePromptIfNeeded('onNeedRefresh-event', updateServiceWorker);

    getToastAction()?.onClick({} as MouseEvent<HTMLButtonElement>);

    vi.advanceTimersByTime(REFRESH_FALLBACK_TIMEOUT_MS);
    await flushPromises();

    expect(locationReload).toHaveBeenCalled();
  });
});
