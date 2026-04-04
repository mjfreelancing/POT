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

const isLaterSnoozeActive = (waitingScriptUrl: string) => {
  // Snooze applies only to the exact service worker key that was deferred.
  // If the key changed, this is a different update and should not be blocked.
  if (
    dismissedWaitingScriptUrl !== waitingScriptUrl ||
    dismissedWaitingScriptAt === undefined
  ) {
    return false;
  }

  return Date.now() - dismissedWaitingScriptAt < LATER_SNOOZE_MS;
};

const hasLaterSnoozeExpired = () => {
  // Without a prior Later action there is no expiry condition to evaluate.
  if (
    dismissedWaitingScriptUrl === undefined ||
    dismissedWaitingScriptAt === undefined
  ) {
    return false;
  }

  return Date.now() - dismissedWaitingScriptAt >= LATER_SNOOZE_MS;
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
  // Reuse the registration from registerSW callback when available to avoid extra lookups.
  if (initialRegistration) {
    return initialRegistration;
  }

  if (serviceWorkerUrl) {
    // Prefer the registration bound to the exact generated SW URL.
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
  // Once refresh starts, suppress all prompt evaluation to avoid duplicate toasts.
  // Skip if refresh was already triggered and we are waiting for SW control/reload.
  if (refreshInProgress) {
    return;
  }

  const waitingScriptUrl = await getWaitingServiceWorkerScriptUrl();
  const deferredScriptUrl = dismissedWaitingScriptUrl;
  const promptScriptUrl = waitingScriptUrl ?? deferredScriptUrl;

  // Normal mode: only prompt when browser reports a waiting worker.
  // Forced mode is used only for deferred re-prompt after Later.
  // No waiting SW means no update prompt should be shown.
  if (!waitingScriptUrl && !forcePromptIfNoWaiting) {
    // Reset dedupe when no waiting worker exists.
    // Service worker script URL is typically stable (/sw.js), so persisting this
    // across cycles can incorrectly suppress future valid prompts.
    promptedWaitingScriptUrl = undefined;
    return;
  }

  if (!promptScriptUrl) {
    // Neither a current waiting worker nor a deferred key exists.
    return;
  }

  // User asked to defer this specific update and the snooze window is still active.
  if (isLaterSnoozeActive(promptScriptUrl)) {
    return;
  }

  // Snooze elapsed for the same key: clear defer state and allow prompt again.
  // If snooze elapsed, allow prompting again for this waiting SW.
  if (dismissedWaitingScriptUrl === promptScriptUrl) {
    dismissedWaitingScriptUrl = undefined;
    dismissedWaitingScriptAt = undefined;

    // Clear dedupe marker so the same waiting SW can be shown again after snooze.
    promptedWaitingScriptUrl = undefined;
  }

  // Avoid duplicate prompts for same waiting SW.
  if (promptedWaitingScriptUrl === promptScriptUrl) {
    return;
  }

  promptedWaitingScriptUrl = promptScriptUrl;

  logger.info('PWA', `Showing update prompt (${trigger})`);

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
            window.clearTimeout(laterSnoozeTimeoutId);
            laterSnoozeTimeoutId = undefined;
          }

          try {
            // Race two signals:
            // 1) updateServiceWorker() completion
            // 2) controllerchange (new worker controlling this page)
            // Either signal can be delayed by browser lifecycle timing, so both have timeouts.
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
            // Timebox this call so a hung updateServiceWorker promise cannot block fallback reload.
            const updateServiceWorkerResult = await Promise.race<
              'completed' | 'timed-out'
            >([
              updateServiceWorker(true).then(() => 'completed'),
              new Promise<'timed-out'>(resolve => {
                window.setTimeout(() => {
                  resolve('timed-out');
                }, REFRESH_FALLBACK_TIMEOUT_MS);
              }),
            ]);

            if (updateServiceWorkerResult === 'timed-out') {
              logger.info(
                'PWA',
                'updateServiceWorker(true) timed out; forcing hard reload fallback',
              );
              // Guarantee user-visible outcome for Refresh click.
              window.location.reload();
              return;
            }

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
              return;
            }

            logger.info(
              'PWA',
              'controllerchange observed after refresh action; forcing hard reload to apply latest assets',
            );
            // Reload even after success so the tab is guaranteed to render latest client bundle.
            window.location.reload();
          } catch (error) {
            // Network/deploy windows and lifecycle races can throw here.
            // Preserve reliability by falling back to hard reload so Refresh always results in an update attempt.
            logger.error(
              'PWA',
              'Service worker update failed during refresh action',
              error,
            );
            // Errors still resolve to a reload so Refresh remains deterministic.
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
          // Only one active snooze timer should exist at a time.
          window.clearTimeout(laterSnoozeTimeoutId);
        }

        // Trigger a re-evaluation when snooze expires so visible tabs can be prompted
        // without waiting for focus changes or the periodic 30-minute update check.
        laterSnoozeTimeoutId = window.setTimeout(() => {
          logger.info(
            'PWA',
            `Later snooze timeout fired (visibility=${document.visibilityState}, service worker key: ${promptScriptUrl})`,
          );

          if (document.visibilityState === 'visible') {
            // Deferred mode allows the prompt to reappear for the same key after Later.
            void showUpdatePromptIfNeeded(
              'later-snooze-expired',
              updateServiceWorker,
              true,
            );

            return;
          }
        }, LATER_SNOOZE_MS);

        toast.dismiss(UPDATE_TOAST_ID);
      },
    },
  });
};

const requestServiceWorkerUpdateCheck = async (
  reason: string,
  serviceWorkerUrl: string,
  initialRegistration?: ServiceWorkerRegistration,
  onWaitingServiceWorkerDetected?: () => Promise<void>,
) => {
  try {
    // registration.update() asks the browser to check whether SW script changed.
    // Trigger a network check for updated SW script.
    const registration = await getServiceWorkerRegistration(
      serviceWorkerUrl,
      initialRegistration,
    );

    if (!registration) {
      return;
    }

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

    promptedWaitingScriptUrl = undefined;
  } catch (error) {
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
      // If user clicked Later and snooze has elapsed while tab was hidden,
      // force a deferred re-prompt as soon as tab is visible again.
      if (hasLaterSnoozeExpired()) {
        void onWaitingServiceWorkerDetected();
      }

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
      // Avoid work while tab is hidden; visibility/focus paths will catch up later.
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

      // Use one callback for all update-check sources so prompt rules stay consistent.
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
