import { createElement } from 'react';

import { RefreshCw } from 'lucide-react';
import { toast } from 'sonner';

import { logger } from '@/concerns/logging';

import {
  isLaterSnoozeActive,
  LATER_SNOOZE_MS,
  pwaRuntimeState,
  REFRESH_FALLBACK_TIMEOUT_MS,
  UPDATE_TOAST_ID,
} from './pwaRuntime';
import { getWaitingServiceWorkerScriptUrl } from './serviceWorkerRegistration';

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
  if (pwaRuntimeState.refreshInProgress) {
    return;
  }

  const waitingScriptUrl = await getWaitingServiceWorkerScriptUrl();
  const deferredScriptUrl = pwaRuntimeState.dismissedWaitingScriptUrl;
  const promptScriptUrl = waitingScriptUrl ?? deferredScriptUrl;

  // Normal mode: only prompt when browser reports a waiting worker.
  // Forced mode is used only for deferred re-prompt after Later.
  // No waiting SW means no update prompt should be shown.
  if (!waitingScriptUrl && !forcePromptIfNoWaiting) {
    // Reset dedupe when no waiting worker exists.
    // Service worker script URL is typically stable (/sw.js), so persisting this
    // across cycles can incorrectly suppress future valid prompts.
    pwaRuntimeState.promptedWaitingScriptUrl = undefined;
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
  if (pwaRuntimeState.dismissedWaitingScriptUrl === promptScriptUrl) {
    pwaRuntimeState.dismissedWaitingScriptUrl = undefined;
    pwaRuntimeState.dismissedWaitingScriptAt = undefined;

    // Clear dedupe marker so the same waiting SW can be shown again after snooze.
    pwaRuntimeState.promptedWaitingScriptUrl = undefined;
  }

  // Avoid duplicate prompts for same waiting SW.
  if (pwaRuntimeState.promptedWaitingScriptUrl === promptScriptUrl) {
    return;
  }

  pwaRuntimeState.promptedWaitingScriptUrl = promptScriptUrl;

  logger.info('PWA', `Showing update prompt (${trigger})`);

  toast(
    createElement(
      'div',
      { className: 'flex items-start gap-3' },
      createElement(RefreshCw, {
        className:
          'mt-0.5 h-6 w-6 shrink-0 text-amber-700 dark:text-amber-300',
      }),
      createElement(
        'div',
        { className: 'space-y-1' },
        createElement(
          'div',
          {
            className:
              'text-base font-semibold leading-tight text-amber-900 dark:text-amber-100',
          },
          'Update Available',
        ),
        createElement(
          'div',
          { className: 'text-sm text-amber-800 dark:text-amber-200' },
          'Refresh to load the latest fixes and improvements.',
        ),
      ),
    ),
    {
      id: UPDATE_TOAST_ID,
      duration: Infinity,
      className:
        '!items-end !border-amber-500/50 !bg-gradient-to-r !from-amber-50 !to-white !shadow-lg dark:!border-amber-400/50 dark:!from-amber-950/60 dark:!to-background',
      classNames: {
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
            pwaRuntimeState.refreshInProgress = true;

            // Once user chooses Refresh, clear any previous "Later" state.
            pwaRuntimeState.dismissedWaitingScriptUrl = undefined;
            pwaRuntimeState.dismissedWaitingScriptAt = undefined;

            if (pwaRuntimeState.laterSnoozeTimeoutId !== undefined) {
              window.clearTimeout(pwaRuntimeState.laterSnoozeTimeoutId);
              pwaRuntimeState.laterSnoozeTimeoutId = undefined;
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
              pwaRuntimeState.refreshInProgress = false;
              pwaRuntimeState.promptedWaitingScriptUrl = undefined;
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
          pwaRuntimeState.dismissedWaitingScriptUrl = promptScriptUrl;
          pwaRuntimeState.dismissedWaitingScriptAt = Date.now();

          // Clear dedupe marker so prompt can return once snooze expires.
          pwaRuntimeState.promptedWaitingScriptUrl = undefined;

          if (pwaRuntimeState.laterSnoozeTimeoutId !== undefined) {
            // Only one active snooze timer should exist at a time.
            window.clearTimeout(pwaRuntimeState.laterSnoozeTimeoutId);
          }

          // Trigger a re-evaluation when snooze expires so visible tabs can be prompted
          // without waiting for focus changes or the periodic 30-minute update check.
          pwaRuntimeState.laterSnoozeTimeoutId = window.setTimeout(() => {
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
    },
  );
};

export { showUpdatePromptIfNeeded };
