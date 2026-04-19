import { beforeEach, describe, expect, test, vi } from 'vitest';

import { logger } from '@/concerns/logging';
import { pwaRuntimeState } from '@/concerns/pwa/pwaRuntime';
import { setupServiceWorkerUpdateChecks } from '@/concerns/pwa/pwaUpdateChecks';
import { showUpdatePromptIfNeeded } from '@/concerns/pwa/pwaUpdatePrompt';
import { registerServiceWorker } from '@/concerns/pwa/registerServiceWorker';
import { registerSW } from 'virtual:pwa-register';

vi.mock('virtual:pwa-register', () => ({
  registerSW: vi.fn(),
}));

vi.mock('@/concerns/logging', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
  },
}));

vi.mock('@/concerns/pwa/pwaUpdateChecks', () => ({
  setupServiceWorkerUpdateChecks: vi.fn(),
}));

vi.mock('@/concerns/pwa/pwaUpdatePrompt', () => ({
  showUpdatePromptIfNeeded: vi.fn(),
}));

type RegisterSwOptions = {
  immediate: boolean;
  onNeedRefresh?: () => void;
  onOfflineReady?: () => void;
  onRegisteredSW?: (
    serviceWorkerUrl: string,
    registration?: ServiceWorkerRegistration,
  ) => void;
  onRegisterError?: (error: unknown) => void;
};

const resetRuntimeState = () => {
  pwaRuntimeState.updateCheckIntervalId = undefined;
  pwaRuntimeState.laterSnoozeTimeoutId = undefined;
  pwaRuntimeState.updateCheckListenersAttached = false;
  pwaRuntimeState.registeredServiceWorkerUrl = undefined;
  pwaRuntimeState.latestServiceWorkerRegistration = undefined;
  pwaRuntimeState.refreshInProgress = false;
  pwaRuntimeState.promptedWaitingScriptUrl = undefined;
  pwaRuntimeState.dismissedWaitingScriptUrl = undefined;
  pwaRuntimeState.dismissedWaitingScriptAt = undefined;
};

const registerInProduction = () => {
  const updateServiceWorker = vi.fn().mockResolvedValue(undefined);
  vi.mocked(registerSW).mockReturnValue(updateServiceWorker);

  registerServiceWorker(false);

  const options = vi.mocked(registerSW).mock.calls[0]?.[0] as
    | RegisterSwOptions
    | undefined;

  return { options, updateServiceWorker };
};

describe('registerServiceWorker', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    resetRuntimeState();
  });

  test('does not register service worker in dev mode', () => {
    registerServiceWorker(true);

    expect(registerSW).not.toHaveBeenCalled();
  });

  test('registers with immediate mode and wires refresh callback', () => {
    const { options, updateServiceWorker } = registerInProduction();

    expect(registerSW).toHaveBeenCalledWith(
      expect.objectContaining({ immediate: true }),
    );

    options?.onNeedRefresh?.();

    expect(showUpdatePromptIfNeeded).toHaveBeenCalledWith(
      'onNeedRefresh-event',
      updateServiceWorker,
    );
  });

  test('logs offline ready and stores registration details', async () => {
    const { options, updateServiceWorker } = registerInProduction();

    const registration = {
      update: vi.fn(),
    } as unknown as ServiceWorkerRegistration;

    options?.onOfflineReady?.();

    expect(logger.info).toHaveBeenCalledWith('PWA', 'Offline cache is ready');

    options?.onRegisteredSW?.('/sw.js', registration);

    expect(pwaRuntimeState.registeredServiceWorkerUrl).toBe('/sw.js');
    expect(pwaRuntimeState.latestServiceWorkerRegistration).toBe(registration);
    expect(logger.info).toHaveBeenCalledWith(
      'PWA',
      'Service worker registered: /sw.js',
    );

    expect(setupServiceWorkerUpdateChecks).toHaveBeenCalledWith(
      '/sw.js',
      expect.any(Function),
    );

    const postCheckCallback = vi.mocked(setupServiceWorkerUpdateChecks).mock
      .calls[0]?.[1];

    await postCheckCallback?.();

    expect(showUpdatePromptIfNeeded).toHaveBeenCalledWith(
      'post-update-check-waiting',
      updateServiceWorker,
    );
  });

  test('logs registration error and clears update interval when present', () => {
    const { options } = registerInProduction();
    const clearIntervalSpy = vi
      .spyOn(window, 'clearInterval')
      .mockImplementation(() => undefined);

    pwaRuntimeState.updateCheckIntervalId = 90210;
    const registrationError = new Error('registration failed');

    options?.onRegisterError?.(registrationError);

    expect(logger.error).toHaveBeenCalledWith(
      'PWA',
      'Service worker registration failed',
      registrationError,
    );

    expect(clearIntervalSpy).toHaveBeenCalledWith(90210);
    expect(pwaRuntimeState.updateCheckIntervalId).toBeUndefined();
  });

  test('does not clear interval when no timer is set during registration error', () => {
    const { options } = registerInProduction();
    const clearIntervalSpy = vi
      .spyOn(window, 'clearInterval')
      .mockImplementation(() => undefined);

    options?.onRegisterError?.(new Error('registration failed'));

    expect(clearIntervalSpy).not.toHaveBeenCalled();
  });
});
