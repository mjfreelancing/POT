import { beforeEach, describe, expect, test, vi } from 'vitest';

import { logger } from '@/concerns/logging';
import {
  LATER_SNOOZE_MS,
  pwaRuntimeState,
  UPDATE_CHECK_INTERVAL_MS,
} from '@/concerns/pwa/pwaRuntime';
import { setupServiceWorkerUpdateChecks } from '@/concerns/pwa/pwaUpdateChecks';
import { getServiceWorkerRegistration } from '@/concerns/pwa/serviceWorkerRegistration';

vi.mock('@/concerns/logging', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
  },
}));

vi.mock('@/concerns/pwa/serviceWorkerRegistration', () => ({
  getServiceWorkerRegistration: vi.fn(),
}));

type RegistrationMock = {
  update: ReturnType<typeof vi.fn>;
  waiting?: ServiceWorker;
};

const createRegistrationMock = (): RegistrationMock => ({
  update: vi.fn().mockResolvedValue(undefined),
});

const flushPromises = async () => {
  await Promise.resolve();
  await Promise.resolve();
};

describe('setupServiceWorkerUpdateChecks', () => {
  let visibilityState: DocumentVisibilityState;
  let focusHandler: (() => void) | undefined;
  let visibilityHandler: (() => void) | undefined;
  let intervalHandler: (() => void) | undefined;

  beforeEach(() => {
    vi.clearAllMocks();

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

    focusHandler = undefined;
    visibilityHandler = undefined;
    intervalHandler = undefined;

    vi.spyOn(window, 'addEventListener').mockImplementation(
      (eventName, listener) => {
        if (eventName === 'focus') {
          focusHandler = listener as () => void;
        }
      },
    );

    vi.spyOn(document, 'addEventListener').mockImplementation(
      (eventName, listener) => {
        if (eventName === 'visibilitychange') {
          visibilityHandler = listener as () => void;
        }
      },
    );

    vi.spyOn(window, 'setInterval').mockImplementation(
      (handler: TimerHandler) => {
        if (typeof handler === 'function') {
          intervalHandler = handler as () => void;
        }

        return 123 as unknown as ReturnType<typeof window.setInterval>;
      },
    );
  });

  test('attaches listeners once and performs startup check', async () => {
    const registration = createRegistrationMock();
    vi.mocked(getServiceWorkerRegistration).mockResolvedValue(
      registration as unknown as ServiceWorkerRegistration,
    );

    const onWaitingServiceWorkerDetected = vi.fn().mockResolvedValue(undefined);

    setupServiceWorkerUpdateChecks('/sw.js', onWaitingServiceWorkerDetected);
    await flushPromises();

    expect(pwaRuntimeState.updateCheckListenersAttached).toBe(true);

    expect(window.addEventListener).toHaveBeenCalledWith(
      'focus',
      expect.any(Function),
    );

    expect(document.addEventListener).toHaveBeenCalledWith(
      'visibilitychange',
      expect.any(Function),
    );

    expect(window.setInterval).toHaveBeenCalledWith(
      expect.any(Function),
      UPDATE_CHECK_INTERVAL_MS,
    );

    expect(registration.update).toHaveBeenCalledTimes(1);

    setupServiceWorkerUpdateChecks('/sw.js', onWaitingServiceWorkerDetected);

    expect(window.addEventListener).toHaveBeenCalledTimes(1);
    expect(document.addEventListener).toHaveBeenCalledTimes(1);
    expect(window.setInterval).toHaveBeenCalledTimes(1);
  });

  test('calls waiting callback when updated registration has waiting worker', async () => {
    const registration = createRegistrationMock();
    registration.waiting = { scriptURL: '/sw.js' } as ServiceWorker;

    vi.mocked(getServiceWorkerRegistration).mockResolvedValue(
      registration as unknown as ServiceWorkerRegistration,
    );

    const onWaitingServiceWorkerDetected = vi.fn().mockResolvedValue(undefined);

    setupServiceWorkerUpdateChecks('/sw.js', onWaitingServiceWorkerDetected);
    await flushPromises();

    expect(onWaitingServiceWorkerDetected).toHaveBeenCalledTimes(1);
    expect(logger.info).toHaveBeenCalledWith(
      'PWA',
      'Waiting service worker detected after update check (startup)',
    );
  });

  test('requests checks on focus, visible tab, and interval when visible', async () => {
    const registration = createRegistrationMock();

    vi.mocked(getServiceWorkerRegistration).mockResolvedValue(
      registration as unknown as ServiceWorkerRegistration,
    );

    const onWaitingServiceWorkerDetected = vi.fn().mockResolvedValue(undefined);

    setupServiceWorkerUpdateChecks('/sw.js', onWaitingServiceWorkerDetected);
    await flushPromises();

    expect(registration.update).toHaveBeenCalledTimes(1);

    focusHandler?.();
    await flushPromises();

    expect(registration.update).toHaveBeenCalledTimes(2);

    visibilityState = 'visible';
    visibilityHandler?.();
    await flushPromises();

    expect(registration.update).toHaveBeenCalledTimes(3);

    intervalHandler?.();
    await flushPromises();

    expect(registration.update).toHaveBeenCalledTimes(4);

    visibilityState = 'hidden';
    intervalHandler?.();
    await flushPromises();

    expect(registration.update).toHaveBeenCalledTimes(4);
  });

  test('re-prompts when later snooze expired and tab becomes visible', async () => {
    const registration = createRegistrationMock();

    vi.mocked(getServiceWorkerRegistration).mockResolvedValue(
      registration as unknown as ServiceWorkerRegistration,
    );

    const onWaitingServiceWorkerDetected = vi.fn().mockResolvedValue(undefined);

    setupServiceWorkerUpdateChecks('/sw.js', onWaitingServiceWorkerDetected);
    await flushPromises();

    pwaRuntimeState.dismissedWaitingScriptUrl = '/sw.js';
    pwaRuntimeState.dismissedWaitingScriptAt = Date.now() - LATER_SNOOZE_MS - 1;

    visibilityState = 'visible';
    visibilityHandler?.();
    await flushPromises();

    expect(onWaitingServiceWorkerDetected).toHaveBeenCalledTimes(1);
  });
});
