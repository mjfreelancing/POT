import { beforeEach, describe, expect, test, vi } from 'vitest';

import { pwaRuntimeState } from '@/concerns/pwa/pwaRuntime';
import {
  getServiceWorkerRegistration,
  getWaitingServiceWorkerScriptUrl,
} from '@/concerns/pwa/serviceWorkerRegistration';

type ServiceWorkerNavigatorMock = {
  getRegistration: ReturnType<typeof vi.fn>;
};

const setServiceWorkerNavigatorMock = (mock: ServiceWorkerNavigatorMock) => {
  Object.defineProperty(navigator, 'serviceWorker', {
    configurable: true,
    value: mock,
  });
};

describe('serviceWorkerRegistration', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    pwaRuntimeState.registeredServiceWorkerUrl = undefined;
  });

  test('returns scoped registration when service worker URL match exists', async () => {
    const scopedRegistration = {
      waiting: undefined,
    } as unknown as ServiceWorkerRegistration;

    const getRegistration = vi
      .fn()
      .mockImplementation(async (scope?: string) => {
        return scope === '/sw.js' ? scopedRegistration : undefined;
      });

    setServiceWorkerNavigatorMock({ getRegistration });

    const result = await getServiceWorkerRegistration('/sw.js');

    expect(result).toBe(scopedRegistration);
    expect(getRegistration).toHaveBeenCalledTimes(1);
    expect(getRegistration).toHaveBeenCalledWith('/sw.js');
  });

  test('falls back to default registration when scoped registration is missing', async () => {
    const defaultRegistration = {
      waiting: undefined,
    } as unknown as ServiceWorkerRegistration;

    const getRegistration = vi
      .fn()
      .mockImplementation(async (scope?: string) => {
        if (scope === '/sw.js') {
          return undefined;
        }

        return defaultRegistration;
      });

    setServiceWorkerNavigatorMock({ getRegistration });

    const result = await getServiceWorkerRegistration('/sw.js');

    expect(result).toBe(defaultRegistration);
    expect(getRegistration).toHaveBeenNthCalledWith(1, '/sw.js');
    expect(getRegistration).toHaveBeenNthCalledWith(2);
  });

  test('returns initial registration when browser lookups return nothing', async () => {
    const initialRegistration = {
      waiting: undefined,
    } as unknown as ServiceWorkerRegistration;

    const getRegistration = vi.fn().mockResolvedValue(undefined);
    setServiceWorkerNavigatorMock({ getRegistration });

    const result = await getServiceWorkerRegistration(
      '/sw.js',
      initialRegistration,
    );

    expect(result).toBe(initialRegistration);
    expect(getRegistration).toHaveBeenNthCalledWith(1, '/sw.js');
    expect(getRegistration).toHaveBeenNthCalledWith(2);
  });

  test('returns waiting service worker script URL when available', async () => {
    pwaRuntimeState.registeredServiceWorkerUrl = '/sw.js';

    const registrationWithWaitingWorker = {
      waiting: {
        scriptURL: '/sw.js',
      },
    } as unknown as ServiceWorkerRegistration;

    const getRegistration = vi
      .fn()
      .mockImplementation(async (scope?: string) => {
        return scope === '/sw.js' ? registrationWithWaitingWorker : undefined;
      });

    setServiceWorkerNavigatorMock({ getRegistration });

    const result = await getWaitingServiceWorkerScriptUrl();

    expect(result).toBe('/sw.js');
  });

  test('returns undefined when no waiting service worker exists', async () => {
    pwaRuntimeState.registeredServiceWorkerUrl = '/sw.js';

    const registrationWithoutWaitingWorker = {
      waiting: undefined,
    } as unknown as ServiceWorkerRegistration;

    const getRegistration = vi
      .fn()
      .mockImplementation(async (scope?: string) => {
        return scope === '/sw.js'
          ? registrationWithoutWaitingWorker
          : undefined;
      });

    setServiceWorkerNavigatorMock({ getRegistration });

    const result = await getWaitingServiceWorkerScriptUrl();

    expect(result).toBeUndefined();
  });
});
