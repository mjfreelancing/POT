import { toast } from 'sonner';
import { registerSW } from 'virtual:pwa-register';

import { logger } from '@/concerns/logging';

// Stable ID for the update toast so we can dismiss/replace one instance instead of stacking multiples.
const UPDATE_TOAST_ID = 'pwa-update-available';

// Shared update timing period used for:
// 1) periodic update checks while visible
// 2) Later snooze duration before re-prompting the same waiting worker
const UPDATE_PROMPT_PERIOD_MS = 1000 * 60 * 30;

const UPDATE_CHECK_INTERVAL_MS = UPDATE_PROMPT_PERIOD_MS;
const LATER_SNOOZE_MS = UPDATE_PROMPT_PERIOD_MS;

// If activation does not lead to controller handoff quickly, force a reload.
// Why this fallback exists:
// 1) The prompt can be shown from deferred state where no real waiting worker exists anymore.
// 2) Service worker lifecycle transitions are asynchronous and can race with user clicks.
// 3) Some browsers delay/suppress immediate controllerchange notifications in edge timings.
// A short timeout keeps the UX responsive while still giving the primary activation path a chance.
const REFRESH_FALLBACK_TIMEOUT_MS = 1500;

// Interval handle for cleanup.
let updateCheckIntervalId: number | undefined;

// Timeout used to re-evaluate update prompt when the Later snooze expires.
let laterSnoozeTimeoutId: number | undefined;

// Guard to ensure focus/visibility/interval listeners are only attached once per page load.
let updateCheckListenersAttached = false;

// The service worker URL returned by registerSW. Used to look up the exact registration later.
let registeredServiceWorkerUrl: string | undefined;

// Prevents re-prompting while the user has already clicked Refresh and activation is in progress.
let refreshInProgress = false;

// Dedupe keys for the current waiting SW script URL.
// prompted: we already showed the toast for this waiting SW.
// dismissed: user clicked Later for this waiting SW and snooze is still active.
let promptedWaitingScriptUrl: string | undefined;
let dismissedWaitingScriptUrl: string | undefined;
let dismissedWaitingScriptAt: number | undefined;

const getServiceWorkerStateSummary = (
  registration: ServiceWorkerRegistration,
) => {
  const waitingWorker = registration.waiting;
  const activeWorker = registration.active;
  const installingWorker = registration.installing;

  return {
    hasWaiting: waitingWorker !== null,
    waitingState: waitingWorker?.state,
    waitingScriptUrl: waitingWorker?.scriptURL,
    hasInstalling: installingWorker !== null,
    installingState: installingWorker?.state,
    installingScriptUrl: installingWorker?.scriptURL,
    hasActive: activeWorker !== null,
    activeState: activeWorker?.state,
    activeScriptUrl: activeWorker?.scriptURL,
  };
};

const isLaterSnoozeActive = (waitingScriptUrl: string) => {
  if (
    dismissedWaitingScriptUrl !== waitingScriptUrl ||
    dismissedWaitingScriptAt === undefined
  ) {
    return false;
  }

  return Date.now() - dismissedWaitingScriptAt < LATER_SNOOZE_MS;
};

// Returns the best matching service worker registration.
// Priority:
// 1) registration passed by callback (fast path)
// 2) registration by exact SW URL scope
// 3) default registration for current page
const getServiceWorkerRegistration = async (
  serviceWorkerUrl?: string,
  initialRegistration?: ServiceWorkerRegistration,
) => {
  if (initialRegistration) {
    return initialRegistration;
  }

  if (serviceWorkerUrl) {
    const scopedRegistration =
      await navigator.serviceWorker.getRegistration(serviceWorkerUrl);

    if (scopedRegistration) {
      return scopedRegistration;
    }
  }

  return navigator.serviceWorker.getRegistration();
};

// Returns the script URL for the waiting service worker (if any).
// Waiting means a new version is installed but not yet controlling the page.
const getWaitingServiceWorkerScriptUrl = async () => {
  const registration = await getServiceWorkerRegistration(
    registeredServiceWorkerUrl,
  );

  if (!registration || !registration.waiting) {
    return undefined;
  }

  return registration.waiting.scriptURL;
};

// Shows the update prompt if all prompt conditions are satisfied.
// This is called from two places:
// - registerSW onNeedRefresh callback
// - manual post-check path after registration.update()
const showUpdatePromptIfNeeded = async (
  trigger: string,
  updateServiceWorker: (reloadPage?: boolean) => Promise<void>,
  forcePromptIfNoWaiting = false,
) => {
  logger.info('PWA', `Evaluating update prompt path (${trigger})`);

  // Skip if refresh was already triggered and we are waiting for SW control/reload.
  if (refreshInProgress) {
    logger.info(
      'PWA',
      `Skipping update prompt (${trigger}): refresh already in progress`,
    );
    return;
  }

  const waitingScriptUrl = await getWaitingServiceWorkerScriptUrl();
  const deferredScriptUrl = dismissedWaitingScriptUrl;
  const promptScriptUrl = waitingScriptUrl ?? deferredScriptUrl;

  // No waiting SW means no update prompt should be shown.
  if (!waitingScriptUrl && !forcePromptIfNoWaiting) {
    const registration = await getServiceWorkerRegistration(
      registeredServiceWorkerUrl,
    );

    if (registration) {
      logger.info(
        'PWA',
        `Skipping update prompt (${trigger}): no waiting service worker; registration state=${JSON.stringify(
          getServiceWorkerStateSummary(registration),
        )}`,
      );
    }

    logger.info(
      'PWA',
      `Skipping update prompt (${trigger}): no waiting service worker`,
    );

    // Reset dedupe when no waiting worker exists.
    // Service worker script URL is typically stable (/sw.js), so persisting this
    // across cycles can incorrectly suppress future valid prompts.
    promptedWaitingScriptUrl = undefined;
    return;
  }

  if (!waitingScriptUrl && forcePromptIfNoWaiting && deferredScriptUrl) {
    logger.info(
      'PWA',
      `Forcing deferred re-prompt after snooze expiry (trigger=${trigger}, deferredSW=${deferredScriptUrl})`,
    );
  }

  if (!promptScriptUrl) {
    logger.info(
      'PWA',
      `Skipping update prompt (${trigger}): no waiting or deferred service worker available`,
    );
    return;
  }

  logger.info('PWA', `Prompt service worker key: ${promptScriptUrl}`);

  // User explicitly deferred this exact waiting SW and snooze is still active.
  if (isLaterSnoozeActive(promptScriptUrl)) {
    const elapsedMs =
      dismissedWaitingScriptAt === undefined
        ? undefined
        : Date.now() - dismissedWaitingScriptAt;

    logger.info(
      'PWA',
      `Skipping update prompt (${trigger}): waiting service worker dismissed (elapsedMs=${elapsedMs}, snoozeMs=${LATER_SNOOZE_MS})`,
    );
    return;
  }

  // If snooze elapsed, allow prompting again for this waiting SW.
  if (dismissedWaitingScriptUrl === promptScriptUrl) {
    logger.info(
      'PWA',
      `Later snooze expired for waiting SW: ${promptScriptUrl}; re-enabling prompt`,
    );

    dismissedWaitingScriptUrl = undefined;
    dismissedWaitingScriptAt = undefined;

    // Clear dedupe marker so the same waiting SW can be shown again after snooze.
    promptedWaitingScriptUrl = undefined;
  }

  // Avoid duplicate prompts for same waiting SW.
  if (promptedWaitingScriptUrl === promptScriptUrl) {
    logger.info(
      'PWA',
      `Skipping update prompt (${trigger}): duplicate waiting service worker prompt`,
    );
    return;
  }

  promptedWaitingScriptUrl = promptScriptUrl;

  logger.info(
    'PWA',
    `Showing update prompt for service worker key: ${promptScriptUrl}`,
  );

  toast.info('Update Available', {
    id: UPDATE_TOAST_ID,
    duration: Infinity,
    description: 'Refresh to load the latest fixes and improvements.',
    className:
      '!border-amber-500/50 !bg-gradient-to-r !from-amber-50 !to-white !shadow-lg dark:!border-amber-400/50 dark:!from-amber-950/60 dark:!to-background',
    descriptionClassName: '!text-amber-800 dark:!text-amber-200',
    classNames: {
      title: '!font-semibold !text-amber-900 dark:!text-amber-100',
      actionButton:
        '!bg-amber-700 !text-white hover:!bg-amber-800 dark:!bg-amber-300 dark:!text-amber-950 dark:hover:!bg-amber-200',
      cancelButton:
        '!border !border-amber-400/70 !bg-transparent !text-amber-900 hover:!bg-amber-100 dark:!border-amber-700 dark:!text-amber-200 dark:hover:!bg-amber-900/50',
    },
    action: {
      label: 'Refresh',
      onClick: () => {
        logger.info(
          'PWA',
          `User clicked Refresh for service worker key: ${promptScriptUrl}`,
        );

        void (async () => {
          // Block additional prompts while activation is underway.
          refreshInProgress = true;

          // Once user chooses Refresh, clear any previous "Later" state.
          dismissedWaitingScriptUrl = undefined;
          dismissedWaitingScriptAt = undefined;

          if (laterSnoozeTimeoutId !== undefined) {
            logger.info(
              'PWA',
              'Clearing existing Later snooze timeout before refresh',
            );
            window.clearTimeout(laterSnoozeTimeoutId);
            laterSnoozeTimeoutId = undefined;
          }

          // Replace update prompt with an explicit in-progress state.
          toast.info('Refreshing...', {
            id: UPDATE_TOAST_ID,
            duration: Infinity,
            description: 'Applying update and reloading...',
          });

          try {
            // We treat controllerchange as the success signal that the new worker took control.
            // The initial updateServiceWorker(true) attempt can appear to "do nothing" when:
            // - there is no waiting worker at click time,
            // - the waiting worker was replaced/cleared between checks,
            // - lifecycle events arrive out of order relative to this click.
            // In those cases, this promise resolves false and we use hard reload fallback.
            const controllerChangedPromise = new Promise<boolean>(resolve => {
              let resolved = false;

              const onControllerChange = () => {
                if (resolved) {
                  return;
                }

                resolved = true;
                navigator.serviceWorker.removeEventListener(
                  'controllerchange',
                  onControllerChange,
                );
                resolve(true);
              };

              navigator.serviceWorker.addEventListener(
                'controllerchange',
                onControllerChange,
              );

              window.setTimeout(() => {
                if (resolved) {
                  return;
                }

                resolved = true;
                navigator.serviceWorker.removeEventListener(
                  'controllerchange',
                  onControllerChange,
                );
                resolve(false);
              }, REFRESH_FALLBACK_TIMEOUT_MS);
            });

            // Primary path: ask Workbox to activate a waiting worker.
            // This is the preferred mechanism because it preserves the SW lifecycle contract.
            await updateServiceWorker(true);
            logger.info(
              'PWA',
              'Requested waiting service worker activation via updateServiceWorker(true)',
            );

            const didControllerChange = await controllerChangedPromise;

            if (!didControllerChange) {
              // Fallback path: activation did not hand off control in time.
              // We still need to fulfill the user action, so force a full reload.
              logger.info(
                'PWA',
                'No controllerchange observed after refresh action; forcing hard reload fallback',
              );
              window.location.reload();
            }
          } catch (error) {
            // Network/deploy windows and lifecycle races can throw here.
            // Preserve reliability by falling back to hard reload so Refresh always results in an update attempt.
            logger.error(
              'PWA',
              'Service worker update failed during refresh action',
              error,
            );

            logger.info(
              'PWA',
              'Falling back to hard reload after refresh action failure',
            );
            window.location.reload();
          } finally {
            // Ensure prompt path can continue in the same page session.
            refreshInProgress = false;
            promptedWaitingScriptUrl = undefined;
          }
        })();
      },
    },
    cancel: {
      label: 'Later',
      onClick: () => {
        logger.info(
          'PWA',
          `User clicked Later for service worker key: ${promptScriptUrl}`,
        );

        // Snooze re-prompting for this same waiting SW for a limited period.
        dismissedWaitingScriptUrl = promptScriptUrl;
        dismissedWaitingScriptAt = Date.now();

        // Clear dedupe marker so prompt can return once snooze expires.
        promptedWaitingScriptUrl = undefined;

        if (laterSnoozeTimeoutId !== undefined) {
          logger.info('PWA', 'Replacing existing Later snooze timeout');
          window.clearTimeout(laterSnoozeTimeoutId);
        }

        logger.info(
          'PWA',
          `Scheduling Later snooze timeout for ${LATER_SNOOZE_MS}ms (service worker key: ${promptScriptUrl})`,
        );

        // Trigger a re-evaluation when snooze expires so visible tabs can be prompted
        // without waiting for focus changes or the periodic 30-minute update check.
        laterSnoozeTimeoutId = window.setTimeout(() => {
          logger.info(
            'PWA',
            `Later snooze timeout fired (visibility=${document.visibilityState}, service worker key: ${promptScriptUrl})`,
          );

          if (document.visibilityState === 'visible') {
            void showUpdatePromptIfNeeded(
              'later-snooze-expired',
              updateServiceWorker,
              true,
            );

            return;
          }

          logger.info(
            'PWA',
            'Skipping later-snooze-expired prompt evaluation because tab is not visible',
          );
        }, LATER_SNOOZE_MS);

        toast.dismiss(UPDATE_TOAST_ID);
      },
    },
  });

  logger.info(
    'PWA',
    `Update toast shown for service worker key: ${promptScriptUrl}`,
  );
};

const requestServiceWorkerUpdateCheck = async (
  reason: string,
  serviceWorkerUrl: string,
  initialRegistration?: ServiceWorkerRegistration,
  onWaitingServiceWorkerDetected?: () => Promise<void>,
) => {
  try {
    logger.info('PWA', `Starting service worker update check (${reason})`);

    // Trigger a network check for updated SW script.
    const registration = await getServiceWorkerRegistration(
      serviceWorkerUrl,
      initialRegistration,
    );

    if (!registration) {
      logger.info(
        'PWA',
        `Service worker update check skipped (${reason}): registration not found`,
      );
      return;
    }

    await registration.update();

    logger.info('PWA', `Service worker update check completed (${reason})`);
    logger.info(
      'PWA',
      `Service worker registration state after update (${reason}): ${JSON.stringify(
        getServiceWorkerStateSummary(registration),
      )}`,
    );

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

    logger.info(
      'PWA',
      `No waiting service worker after update check (${reason})`,
    );

    promptedWaitingScriptUrl = undefined;
  } catch (error) {
    logger.error(
      'PWA',
      `Failed service worker update check (${reason})`,
      error,
    );
  }
};

const setupServiceWorkerUpdateChecks = (
  serviceWorkerUrl: string,
  registration: ServiceWorkerRegistration | undefined,
  onWaitingServiceWorkerDetected: () => Promise<void>,
) => {
  // Avoid duplicate listeners (possible if initialization path changes in future).
  if (updateCheckListenersAttached) {
    return;
  }

  updateCheckListenersAttached = true;

  // Immediate startup check catches updates deployed before this session opened.
  void requestServiceWorkerUpdateCheck(
    'startup',
    serviceWorkerUrl,
    registration,
    onWaitingServiceWorkerDetected,
  );

  // Re-check when user returns to the window.
  window.addEventListener('focus', () => {
    void requestServiceWorkerUpdateCheck(
      'window-focus',
      serviceWorkerUrl,
      registration,
      onWaitingServiceWorkerDetected,
    );
  });

  // Re-check when tab becomes visible again.
  document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible') {
      void requestServiceWorkerUpdateCheck(
        'tab-visible',
        serviceWorkerUrl,
        registration,
        onWaitingServiceWorkerDetected,
      );
      return;
    }
  });

  // Periodic checks while visible so long-lived tabs still discover updates.
  updateCheckIntervalId = window.setInterval(() => {
    if (document.visibilityState === 'visible') {
      void requestServiceWorkerUpdateCheck(
        'interval',
        serviceWorkerUrl,
        registration,
        onWaitingServiceWorkerDetected,
      );
      return;
    }
  }, UPDATE_CHECK_INTERVAL_MS);
};

// Called once at application startup from main.tsx.
// Registers SW lifecycle hooks and wires update detection + user prompt behavior.
const registerServiceWorker = () => {
  // Do not run this flow in Vite dev mode.
  if (import.meta.env.DEV) {
    return;
  }

  const updateServiceWorker = registerSW({
    immediate: true,

    // Primary plugin event for "new SW is waiting".
    onNeedRefresh() {
      void showUpdatePromptIfNeeded('onNeedRefresh-event', updateServiceWorker);
    },

    // Fired once offline assets are ready for the first time.
    onOfflineReady() {
      logger.info('PWA', 'Offline cache is ready');
    },

    // Registration success: capture URL and start proactive checks.
    onRegisteredSW(serviceWorkerUrl, registration) {
      registeredServiceWorkerUrl = serviceWorkerUrl;
      logger.info('PWA', `Service worker registered: ${serviceWorkerUrl}`);
      setupServiceWorkerUpdateChecks(serviceWorkerUrl, registration, async () =>
        showUpdatePromptIfNeeded(
          'post-update-check-waiting',
          updateServiceWorker,
        ),
      );
    },

    // Registration failure: log and clean interval timer if it exists.
    onRegisterError(error) {
      logger.error('PWA', 'Service worker registration failed', error);

      if (updateCheckIntervalId !== undefined) {
        window.clearInterval(updateCheckIntervalId);
        updateCheckIntervalId = undefined;
      }
    },
  });
};

export { registerServiceWorker };
