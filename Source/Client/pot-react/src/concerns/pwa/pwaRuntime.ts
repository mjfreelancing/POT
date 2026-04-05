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

const pwaRuntimeState = {
  updateCheckIntervalId: undefined as number | undefined,
  laterSnoozeTimeoutId: undefined as number | undefined,
  updateCheckListenersAttached: false,
  registeredServiceWorkerUrl: undefined as string | undefined,
  latestServiceWorkerRegistration: undefined as
    | ServiceWorkerRegistration
    | undefined,
  refreshInProgress: false,
  promptedWaitingScriptUrl: undefined as string | undefined,
  dismissedWaitingScriptUrl: undefined as string | undefined,
  dismissedWaitingScriptAt: undefined as number | undefined,
};

const isLaterSnoozeActive = (waitingScriptUrl: string) => {
  // Snooze applies only to the exact service worker key that was deferred.
  // If the key changed, this is a different update and should not be blocked.
  if (
    pwaRuntimeState.dismissedWaitingScriptUrl !== waitingScriptUrl ||
    pwaRuntimeState.dismissedWaitingScriptAt === undefined
  ) {
    return false;
  }

  return (
    Date.now() - pwaRuntimeState.dismissedWaitingScriptAt < LATER_SNOOZE_MS
  );
};

const hasLaterSnoozeExpired = () => {
  // Without a prior Later action there is no expiry condition to evaluate.
  if (
    pwaRuntimeState.dismissedWaitingScriptUrl === undefined ||
    pwaRuntimeState.dismissedWaitingScriptAt === undefined
  ) {
    return false;
  }

  return (
    Date.now() - pwaRuntimeState.dismissedWaitingScriptAt >= LATER_SNOOZE_MS
  );
};

export {
  hasLaterSnoozeExpired,
  isLaterSnoozeActive,
  LATER_SNOOZE_MS,
  pwaRuntimeState,
  REFRESH_FALLBACK_TIMEOUT_MS,
  UPDATE_CHECK_INTERVAL_MS,
  UPDATE_TOAST_ID,
};
