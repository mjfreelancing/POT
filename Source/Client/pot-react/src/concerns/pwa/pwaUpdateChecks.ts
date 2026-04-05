import { logger } from '@/concerns/logging';

import {
  hasLaterSnoozeExpired,
  pwaRuntimeState,
  UPDATE_CHECK_INTERVAL_MS,
} from './pwaRuntime';
import { getServiceWorkerRegistration } from './serviceWorkerRegistration';

const requestServiceWorkerUpdateCheck = async (
  reason: string,
  serviceWorkerUrl: string,
  onWaitingServiceWorkerDetected?: () => Promise<void>,
) => {
  const runRegistrationUpdate = async (
    registration: ServiceWorkerRegistration,
  ) => {
    await registration.update();

    // If waiting exists now, surface update prompt through shared handler path.
    // Some browsers may not fire onNeedRefresh immediately in all timing scenarios.
    // This explicit path ensures we still prompt if a waiting SW exists after update check.
    if (registration.waiting) {
      logger.info(
        'PWA',
        `Waiting service worker detected after update check (${reason})`,
      );

      await onWaitingServiceWorkerDetected?.();

      return;
    }

    pwaRuntimeState.promptedWaitingScriptUrl = undefined;
  };

  try {
    // registration.update() asks the browser to check whether SW script changed.
    // Trigger a network check for updated SW script.
    const registration = await getServiceWorkerRegistration(
      serviceWorkerUrl,
      pwaRuntimeState.latestServiceWorkerRegistration,
    );

    if (!registration) {
      return;
    }

    await runRegistrationUpdate(registration);
  } catch (error) {
    if (error instanceof DOMException && error.name === 'InvalidStateError') {
      try {
        // Registration object can become stale after unregister/re-register.
        // Retry once with a fresh lookup to keep focus/visibility checks resilient.
        const freshRegistration =
          await getServiceWorkerRegistration(serviceWorkerUrl);

        if (freshRegistration) {
          pwaRuntimeState.latestServiceWorkerRegistration = freshRegistration;
          await runRegistrationUpdate(freshRegistration);
          return;
        }
      } catch {
        // Fall through to the shared error log below.
      }
    }

    // Keep hard failures visible during troubleshooting.
    logger.error(
      'PWA',
      `Failed service worker update check (${reason})`,
      error,
    );
  }
};

const setupServiceWorkerUpdateChecks = (
  serviceWorkerUrl: string,
  onWaitingServiceWorkerDetected: () => Promise<void>,
) => {
  // Avoid duplicate listeners (possible if initialization path changes in future).
  if (pwaRuntimeState.updateCheckListenersAttached) {
    return;
  }

  pwaRuntimeState.updateCheckListenersAttached = true;

  // Immediate startup check catches updates deployed before this session opened.
  void requestServiceWorkerUpdateCheck(
    'startup',
    serviceWorkerUrl,
    onWaitingServiceWorkerDetected,
  );

  // Re-check when user returns to the window.
  window.addEventListener('focus', () => {
    void requestServiceWorkerUpdateCheck(
      'window-focus',
      serviceWorkerUrl,
      onWaitingServiceWorkerDetected,
    );
  });

  // Re-check when tab becomes visible again.
  document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible') {
      // If user clicked Later and snooze has elapsed while tab was hidden,
      // force a deferred re-prompt as soon as tab is visible again.
      if (hasLaterSnoozeExpired()) {
        void onWaitingServiceWorkerDetected();
      }

      void requestServiceWorkerUpdateCheck(
        'tab-visible',
        serviceWorkerUrl,
        onWaitingServiceWorkerDetected,
      );
      return;
    }
  });

  // Periodic checks while visible so long-lived tabs still discover updates.
  pwaRuntimeState.updateCheckIntervalId = window.setInterval(() => {
    if (document.visibilityState === 'visible') {
      // Avoid work while tab is hidden; visibility/focus paths will catch up later.
      void requestServiceWorkerUpdateCheck(
        'interval',
        serviceWorkerUrl,
        onWaitingServiceWorkerDetected,
      );
      return;
    }
  }, UPDATE_CHECK_INTERVAL_MS);
};

export { setupServiceWorkerUpdateChecks };
