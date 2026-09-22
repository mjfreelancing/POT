import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

import { logger } from '@/concerns/logging';
import {
  pwaRuntimeState,
  VERSION_CHECK_INTERVAL_MS,
  VERSION_FILE_URL,
} from '@/concerns/pwa/pwaRuntime';
import { startVersionChecks } from '@/concerns/pwa/pwaVersionCheck';

vi.mock('@/concerns/logging', () => ({
  logger: {
    info: vi.fn(),
    warn: vi.fn(),
    error: vi.fn(),
  },
}));

const CLIENT_BUILD_ID = 'build-running';

const fetchMock = vi.fn();

const resetRuntimeState = () => {
  pwaRuntimeState.versionCheckIntervalId = undefined;
  pwaRuntimeState.versionCheckListenersAttached = false;
  pwaRuntimeState.versionCheckInFlight = false;
  pwaRuntimeState.handledRemoteBuildId = undefined;
};

const mockDeployedBuild = (buildId: unknown) => {
  fetchMock.mockResolvedValue({
    ok: true,
    json: async () => ({ buildId }),
  });
};

const mockDeployedBuildFailure = () => {
  fetchMock.mockResolvedValue({
    ok: false,
    status: 404,
    json: async () => ({}),
  });
};

// Drains pending microtasks plus a macrotask, so a version check has fully settled (including the
// in-flight guard being released) before the next trigger fires.
const flushPromises = async () => {
  await new Promise(resolve => {
    setTimeout(resolve, 0);
  });
};

const startChecks = () => {
  const onVersionChanged = vi.fn().mockResolvedValue(undefined);

  startVersionChecks({ clientBuildId: CLIENT_BUILD_ID, onVersionChanged });

  return onVersionChanged;
};

describe('startVersionChecks', () => {
  let visibilityState: DocumentVisibilityState;
  let focusHandler: (() => void) | undefined;
  let visibilityHandler: (() => void) | undefined;
  let intervalHandler: (() => void) | undefined;

  beforeEach(() => {
    vi.clearAllMocks();
    vi.stubGlobal('fetch', fetchMock);
    resetRuntimeState();

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

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  test('does not start checks when the running build id is missing', () => {
    startVersionChecks({
      clientBuildId: undefined,
      onVersionChanged: vi.fn(),
    });

    expect(fetchMock).not.toHaveBeenCalled();
    expect(window.addEventListener).not.toHaveBeenCalled();
    expect(window.setInterval).not.toHaveBeenCalled();
    expect(pwaRuntimeState.versionCheckListenersAttached).toBe(false);

    expect(logger.info).toHaveBeenCalledWith(
      'PWA',
      'Deployed build checks disabled (no client build id)',
    );
  });

  test('checks the deployed build at startup and attaches its triggers once', async () => {
    mockDeployedBuild(CLIENT_BUILD_ID);

    const onVersionChanged = startChecks();
    await flushPromises();

    expect(fetchMock).toHaveBeenCalledWith(VERSION_FILE_URL, {
      cache: 'no-store',
    });

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
      VERSION_CHECK_INTERVAL_MS,
    );

    expect(onVersionChanged).not.toHaveBeenCalled();

    startChecks();
    await flushPromises();

    expect(window.addEventListener).toHaveBeenCalledTimes(1);
    expect(window.setInterval).toHaveBeenCalledTimes(1);
  });

  test('calls back when the deployed build differs from the running build', async () => {
    mockDeployedBuild('build-deployed');

    const onVersionChanged = startChecks();
    await flushPromises();

    expect(onVersionChanged).toHaveBeenCalledTimes(1);
    expect(pwaRuntimeState.handledRemoteBuildId).toBe('build-deployed');

    expect(logger.info).toHaveBeenCalledWith(
      'PWA',
      'Deployed build check (startup): deployed build differs, checking for a service worker update (running=build-running, deployed=build-deployed)',
    );
  });

  test('does not call back when the deployed build matches the running build', async () => {
    mockDeployedBuild(CLIENT_BUILD_ID);

    const onVersionChanged = startChecks();
    await flushPromises();

    expect(onVersionChanged).not.toHaveBeenCalled();
    expect(logger.info).toHaveBeenCalledWith(
      'PWA',
      'Deployed build check (startup): up to date (running=build-running)',
    );
  });

  test('does not call back when the deployed build id is absent from the file', async () => {
    mockDeployedBuild(undefined);

    const onVersionChanged = startChecks();
    await flushPromises();

    expect(onVersionChanged).not.toHaveBeenCalled();
  });

  test('does not call back twice for the same deployed build', async () => {
    mockDeployedBuild('build-deployed');

    const onVersionChanged = startChecks();
    await flushPromises();

    focusHandler?.();
    await flushPromises();

    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(onVersionChanged).toHaveBeenCalledTimes(1);

    expect(logger.info).toHaveBeenCalledWith(
      'PWA',
      'Deployed build check (window-focus): deployed build already handled (deployed=build-deployed)',
    );
  });

  test('calls back again when a newer deployed build appears', async () => {
    mockDeployedBuild('build-deployed-2');

    const onVersionChanged = startChecks();
    await flushPromises();

    mockDeployedBuild('build-deployed-3');
    focusHandler?.();
    await flushPromises();

    expect(onVersionChanged).toHaveBeenCalledTimes(2);
  });

  test('checks on window focus and when the tab becomes visible', async () => {
    mockDeployedBuild(CLIENT_BUILD_ID);
    startChecks();
    await flushPromises();

    focusHandler?.();
    await flushPromises();

    visibilityState = 'visible';
    visibilityHandler?.();
    await flushPromises();

    expect(fetchMock).toHaveBeenCalledTimes(3);
  });

  test('skips checks while the tab is hidden', async () => {
    mockDeployedBuild(CLIENT_BUILD_ID);
    startChecks();
    await flushPromises();

    visibilityState = 'hidden';

    visibilityHandler?.();
    intervalHandler?.();
    await flushPromises();

    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  test('checks on the interval while the tab is visible', async () => {
    mockDeployedBuild(CLIENT_BUILD_ID);
    startChecks();
    await flushPromises();

    intervalHandler?.();
    await flushPromises();

    expect(fetchMock).toHaveBeenCalledTimes(2);
  });

  test('runs a single check when triggers fire while a check is in flight', async () => {
    let resolveDeployedBuild: ((response: Response) => void) | undefined;

    fetchMock.mockReturnValue(
      new Promise<Response>(resolve => {
        resolveDeployedBuild = resolve;
      }),
    );

    startChecks();
    focusHandler?.();
    visibilityHandler?.();

    expect(fetchMock).toHaveBeenCalledTimes(1);

    expect(logger.info).toHaveBeenCalledWith(
      'PWA',
      'Deployed build check (window-focus): skipped, a check is already in flight',
    );

    resolveDeployedBuild?.({
      ok: true,
      json: async () => ({ buildId: CLIENT_BUILD_ID }),
    } as unknown as Response);

    await flushPromises();

    expect(pwaRuntimeState.versionCheckInFlight).toBe(false);
  });

  test('logs a warning and keeps the client usable when the check fails', async () => {
    const checkError = new Error('offline');
    fetchMock.mockRejectedValue(checkError);

    const onVersionChanged = startChecks();
    await flushPromises();

    expect(onVersionChanged).not.toHaveBeenCalled();
    expect(logger.warn).toHaveBeenCalledWith(
      'PWA',
      'Deployed build check failed (startup)',
      checkError,
    );
  });

  test('logs a warning when the deployed build file is unavailable', async () => {
    mockDeployedBuildFailure();

    const onVersionChanged = startChecks();
    await flushPromises();

    expect(onVersionChanged).not.toHaveBeenCalled();
    expect(logger.warn).toHaveBeenCalledWith(
      'PWA',
      'Deployed build check failed (startup)',
      expect.objectContaining({ message: 'Unexpected status 404' }),
    );
  });
});
