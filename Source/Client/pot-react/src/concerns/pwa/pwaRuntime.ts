// Stable ID for the update toast so we can dismiss/replace one instance instead of stacking multiples.
const UPDATE_TOAST_ID = 'pwa-update-available';

// How often an active tab checks for a new service worker.
const UPDATE_CHECK_INTERVAL_MS = 1000 * 60 * 30;

// A detected update is applied once the user has been quiet for this long.
// Applying during a quiet moment avoids discarding a partially typed form where possible.
const UPDATE_QUIET_PERIOD_MS = 1000 * 45;

// Hard cap on how long an update can be deferred. An update must always be applied, so this
// trigger wins over the quiet period even while the user keeps interacting.
const UPDATE_MAX_DEFER_MS = 1000 * 60 * 5;

// Evaluation cadence for deciding whether a pending update is safe to apply.
const UPDATE_ENFORCEMENT_TICK_MS = 1000;

// If activation does not lead to controller handoff quickly, force a reload.
// Why this fallback exists:
// 1) The prompt can be shown from deferred state where no real waiting worker exists anymore.
// 2) Service worker lifecycle transitions are asynchronous and can race with user clicks.
// 3) Some browsers delay/suppress immediate controllerchange notifications in edge timings.
// A short timeout keeps the UX responsive while still giving the primary activation path a chance.
const REFRESH_FALLBACK_TIMEOUT_MS = 1500;

const pwaRuntimeState = {
  updateCheckIntervalId: undefined as number | undefined,
  enforcementIntervalId: undefined as number | undefined,
  updateCheckListenersAttached: false,
  activityListenersAttached: false,
  registeredServiceWorkerUrl: undefined as string | undefined,
  latestServiceWorkerRegistration: undefined as
    ServiceWorkerRegistration | undefined,
  refreshInProgress: false,
  promptedWaitingScriptUrl: undefined as string | undefined,
  pendingUpdateScriptUrl: undefined as string | undefined,
  pendingUpdateDetectedAt: undefined as number | undefined,
  lastUserActivityAt: undefined as number | undefined,
};

type PendingUpdateTiming = {
  detectedAt: number;
  lastUserActivityAt: number;
  now: number;
};

// A pending update is applied when either trigger fires:
// 1) The user has been quiet for UPDATE_QUIET_PERIOD_MS, which means nothing is mid-edit.
//    A hidden tab receives no input events, so this also covers "the user left the tab".
// 2) UPDATE_MAX_DEFER_MS has elapsed since detection, because an update must always be applied
//    even while the user keeps interacting.
function isPendingUpdateReadyToApply({
  detectedAt,
  lastUserActivityAt,
  now,
}: PendingUpdateTiming) {
  if (now - detectedAt >= UPDATE_MAX_DEFER_MS) {
    return true;
  }

  return now - lastUserActivityAt >= UPDATE_QUIET_PERIOD_MS;
}

export {
  isPendingUpdateReadyToApply,
  pwaRuntimeState,
  REFRESH_FALLBACK_TIMEOUT_MS,
  UPDATE_CHECK_INTERVAL_MS,
  UPDATE_ENFORCEMENT_TICK_MS,
  UPDATE_MAX_DEFER_MS,
  UPDATE_QUIET_PERIOD_MS,
  UPDATE_TOAST_ID,
};
