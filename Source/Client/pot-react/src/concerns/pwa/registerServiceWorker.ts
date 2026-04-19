import { registerSW } from 'virtual:pwa-register';

import { logger } from '@/concerns/logging';

import { pwaRuntimeState } from './pwaRuntime';
import { setupServiceWorkerUpdateChecks } from './pwaUpdateChecks';
import { showUpdatePromptIfNeeded } from './pwaUpdatePrompt';

// Called once at application startup from main.tsx.
// Registers SW lifecycle hooks and wires update detection + user prompt behavior.
const registerServiceWorker = (isDevelopment = import.meta.env.DEV) => {
  // Do not run this flow in Vite dev mode.
  if (isDevelopment) {
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
      pwaRuntimeState.registeredServiceWorkerUrl = serviceWorkerUrl;
      pwaRuntimeState.latestServiceWorkerRegistration = registration;
      logger.info('PWA', `Service worker registered: ${serviceWorkerUrl}`);

      // Use one callback for all update-check sources so prompt rules stay consistent.
      setupServiceWorkerUpdateChecks(serviceWorkerUrl, async () =>
        showUpdatePromptIfNeeded(
          'post-update-check-waiting',
          updateServiceWorker,
        ),
      );
    },

    // Registration failure: log and clean interval timer if it exists.
    onRegisterError(error) {
      logger.error('PWA', 'Service worker registration failed', error);

      if (pwaRuntimeState.updateCheckIntervalId !== undefined) {
        window.clearInterval(pwaRuntimeState.updateCheckIntervalId);
        pwaRuntimeState.updateCheckIntervalId = undefined;
      }
    },
  });
};

export { registerServiceWorker };
