import { RefreshCw } from 'lucide-react';
import { createElement } from 'react';
import { toast } from 'sonner';

import { logger } from '@/concerns/logging';

import {
  pwaRuntimeState,
  REFRESH_FALLBACK_TIMEOUT_MS,
  UPDATE_TOAST_ID,
} from './pwaRuntime';
import { startUpdateEnforcement } from './pwaUpdateEnforcement';
import { getWaitingServiceWorkerScriptUrl } from './serviceWorkerRegistration';

// Shows the update prompt if all prompt conditions are satisfied.
// This is called from two places:
// - registerSW onNeedRefresh callback
// - manual post-check path after registration.update()
const showUpdatePromptIfNeeded = async (
  trigger: string,
  updateServiceWorker: (reloadPage?: boolean) => Promise<void>,
) => {
  // Once refresh starts, suppress all prompt evaluation to avoid duplicate toasts.
  // Skip if refresh was already triggered and we are waiting for SW control/reload.
  if (pwaRuntimeState.refreshInProgress) {
    return;
  }

  const waitingScriptUrl = await getWaitingServiceWorkerScriptUrl();

  // No waiting SW means no update prompt should be shown.
  if (!waitingScriptUrl) {
    // Reset dedupe when no waiting worker exists.
    // Service worker script URL is typically stable (/sw.js), so persisting this
    // across cycles can incorrectly suppress future valid prompts.
    pwaRuntimeState.promptedWaitingScriptUrl = undefined;
    return;
  }

  // Avoid duplicate prompts for same waiting SW.
  if (pwaRuntimeState.promptedWaitingScriptUrl === waitingScriptUrl) {
    return;
  }

  pwaRuntimeState.promptedWaitingScriptUrl = waitingScriptUrl;

  logger.info('PWA', `Showing update prompt (${trigger})`);

  // Applies the detected update and guarantees a reload. Shared by the prompt action and by
  // automatic enforcement, because an update must always be applied.
  const applyUpdate = () => {
    logger.info(
      'PWA',
      `Applying service worker update for key: ${waitingScriptUrl}`,
    );

    // Block additional prompts while activation is underway.
    pwaRuntimeState.refreshInProgress = true;

    void (async () => {
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
  };

  toast(
    createElement(
      'div',
      { className: 'flex items-start gap-3' },
      createElement(RefreshCw, {
        className: 'mt-0.5 h-6 w-6 shrink-0 text-amber-700 dark:text-amber-300',
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
          'Unsaved edits will be lost',
        ),
      ),
    ),
    {
      id: UPDATE_TOAST_ID,
      duration: Infinity,
      // An update must always be applied, so the prompt cannot be deferred or dismissed.
      dismissible: false,
      className:
        '!items-end !border-amber-500/50 !bg-gradient-to-r !from-amber-50 !to-white !shadow-lg dark:!border-amber-400/50 dark:!from-amber-950/60 dark:!to-background',
      classNames: {
        actionButton:
          '!bg-amber-700 !text-white hover:!bg-amber-800 dark:!bg-amber-300 dark:!text-amber-950 dark:hover:!bg-amber-200',
      },
      action: {
        label: 'Update now',
        onClick: applyUpdate,
      },
    },
  );

  // The prompt action is the fastest path, but an update must always be applied, so automatic
  // enforcement is started for every detected update.
  startUpdateEnforcement(waitingScriptUrl, applyUpdate);
};

export { showUpdatePromptIfNeeded };
