import { logger } from '@/concerns/logging';

import {
  isPendingUpdateReadyToApply,
  pwaRuntimeState,
  UPDATE_ENFORCEMENT_TICK_MS,
} from './pwaRuntime';

// Input events that indicate the user is actively working in the app.
const ACTIVITY_EVENT_NAMES = ['keydown', 'input', 'pointerdown'] as const;

const markUserActivity = () => {
  pwaRuntimeState.lastUserActivityAt = Date.now();
};

// Listeners stay attached for the session: they only record a timestamp, and keeping them
// avoids re-attaching for every update cycle.
const attachActivityListeners = () => {
  if (pwaRuntimeState.activityListenersAttached) {
    return;
  }

  pwaRuntimeState.activityListenersAttached = true;

  for (const eventName of ACTIVITY_EVENT_NAMES) {
    window.addEventListener(eventName, markUserActivity, { passive: true });
  }
};

const stopUpdateEnforcement = () => {
  if (pwaRuntimeState.enforcementIntervalId !== undefined) {
    window.clearInterval(pwaRuntimeState.enforcementIntervalId);
    pwaRuntimeState.enforcementIntervalId = undefined;
  }

  pwaRuntimeState.pendingUpdateScriptUrl = undefined;
  pwaRuntimeState.pendingUpdateDetectedAt = undefined;
};

// An update must always be applied, so a detected update is enforced until it is applied.
// The user can apply it sooner through the prompt action; otherwise it is applied at the first
// quiet moment (or at the maximum deferral window when the user keeps interacting).
const startUpdateEnforcement = (scriptUrl: string, applyUpdate: () => void) => {
  // Only one pending update is enforced at a time.
  stopUpdateEnforcement();

  const detectedAt = Date.now();

  pwaRuntimeState.pendingUpdateScriptUrl = scriptUrl;
  pwaRuntimeState.pendingUpdateDetectedAt = detectedAt;
  // Detection time starts the quiet window so a prompt is never applied instantly.
  pwaRuntimeState.lastUserActivityAt = detectedAt;

  attachActivityListeners();

  pwaRuntimeState.enforcementIntervalId = window.setInterval(() => {
    const { pendingUpdateDetectedAt, lastUserActivityAt } = pwaRuntimeState;

    if (
      pendingUpdateDetectedAt === undefined ||
      lastUserActivityAt === undefined
    ) {
      return;
    }

    const isReadyToApply = isPendingUpdateReadyToApply({
      detectedAt: pendingUpdateDetectedAt,
      lastUserActivityAt,
      now: Date.now(),
    });

    if (!isReadyToApply) {
      return;
    }

    logger.info(
      'PWA',
      `Applying pending update automatically for service worker key: ${scriptUrl}`,
    );

    stopUpdateEnforcement();
    applyUpdate();
  }, UPDATE_ENFORCEMENT_TICK_MS);
};

export { startUpdateEnforcement, stopUpdateEnforcement };
