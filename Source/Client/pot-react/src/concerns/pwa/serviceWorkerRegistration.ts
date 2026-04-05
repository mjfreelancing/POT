import { pwaRuntimeState } from './pwaRuntime';

// Returns the best matching service worker registration.
// Priority:
// 1) registration passed by callback (fast path)
// 2) registration by exact SW URL scope
// 3) default registration for current page
const getServiceWorkerRegistration = async (
  serviceWorkerUrl?: string,
  initialRegistration?: ServiceWorkerRegistration,
) => {
  if (serviceWorkerUrl) {
    // Prefer the registration bound to the exact generated SW URL.
    const scopedRegistration =
      await navigator.serviceWorker.getRegistration(serviceWorkerUrl);

    if (scopedRegistration) {
      return scopedRegistration;
    }
  }

  const defaultRegistration = await navigator.serviceWorker.getRegistration();

  if (defaultRegistration) {
    return defaultRegistration;
  }

  // Fallback only when browser lookups do not return a registration.
  return initialRegistration;
};

// Returns the script URL for the waiting service worker (if any).
// Waiting means a new version is installed but not yet controlling the page.
const getWaitingServiceWorkerScriptUrl = async () => {
  const registration = await getServiceWorkerRegistration(
    pwaRuntimeState.registeredServiceWorkerUrl,
  );

  if (!registration || !registration.waiting) {
    return undefined;
  }

  return registration.waiting.scriptURL;
};

export { getServiceWorkerRegistration, getWaitingServiceWorkerScriptUrl };
