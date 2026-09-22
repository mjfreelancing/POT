import { beforeEach, describe, expect, test, vi } from 'vitest';

import { logger } from '@/concerns/logging';
import {
  pwaRuntimeState,
  UPDATE_CHECK_INTERVAL_MS,
} from '@/concerns/pwa/pwaRuntime';
import { setupServiceWorkerUpdateChecks } from '@/concerns/pwa/pwaUpdateChecks';
import { startVersionChecks } from '@/concerns/pwa/pwaVersionCheck';
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

vi.mock('@/concerns/pwa/pwaVersionCheck', () => ({
  startVersionChecks: vi.fn(),
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
    pwaRuntimeState.enforcementIntervalId = undefined;
    pwaRuntimeState.updateCheckListenersAttached = false;
    pwaRuntimeState.registeredServiceWorkerUrl = undefined;
    pwaRuntimeState.latestServiceWorkerRegistration = undefined;
    pwaRuntimeState.refreshInProgress = false;
    pwaRuntimeState.promptedWaitingScriptUrl = undefined;
    pwaRuntimeState.pendingUpdateScriptUrl = undefined;
    pwaRuntimeState.pendingUpdateDetectedAt = undefined;
    pwaRuntimeState.lastUserActivityAt = undefined;

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

  test('starts deployed-build checks that reuse the service worker check path', async () => {
    const registration = createRegistrationMock();
    vi.mocked(getServiceWorkerRegistration).mockResolvedValue(
      registration as unknown as ServiceWorkerRegistration,
    );

    const onWaitingServiceWorkerDetected = vi.fn().mockResolvedValue(undefined);

    setupServiceWorkerUpdateChecks(
      '/sw.js',
      onWaitingServiceWorkerDetected,
      'build-1',
    );
    await flushPromises();

    expect(startVersionChecks).toHaveBeenCalledWith({
      clientBuildId: 'build-1',
      onVersionChanged: expect.any(Function),
    });

    const versionCheckOptions =
      vi.mocked(startVersionChecks).mock.calls[0]?.[0];

    await versionCheckOptions?.onVersionChanged();

    // The deployed-build channel cannot apply anything itself, so it runs the same check path.
    expect(registration.update).toHaveBeenCalledTimes(2);
    expect(onWaitingServiceWorkerDetected).not.toHaveBeenCalled();
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
      'Service worker update check (startup): waiting service worker detected',
    );
  });

  test('logs the outcome when the check finds no waiting worker', async () => {
    const registration = createRegistrationMock();

    vi.mocked(getServiceWorkerRegistration).mockResolvedValue(
      registration as unknown as ServiceWorkerRegistration,
    );

    setupServiceWorkerUpdateChecks(
      '/sw.js',
      vi.fn().mockResolvedValue(undefined),
    );
    await flushPromises();

    expect(logger.info).toHaveBeenCalledWith(
      'PWA',
      'Service worker update check (startup): no waiting service worker',
    );
  });

  test('logs the outcome when no registration is available', async () => {
    vi.mocked(getServiceWorkerRegistration).mockResolvedValue(undefined);

    setupServiceWorkerUpdateChecks(
      '/sw.js',
      vi.fn().mockResolvedValue(undefined),
    );
    await flushPromises();

    expect(logger.info).toHaveBeenCalledWith(
      'PWA',
      'Service worker update check (startup): no registration available',
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
});
