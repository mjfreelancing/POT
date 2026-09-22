import { logger } from '@/concerns/logging';

import {
  pwaRuntimeState,
  VERSION_CHECK_INTERVAL_MS,
  VERSION_FILE_URL,
} from './pwaRuntime';

type VersionCheckOptions = {
  // Identity of the running build. Undefined outside a build that bakes it in.
  clientBuildId: string | undefined;
  // Called when the deployed build differs from the running build. It must not apply the update
  // itself, because only the service worker can fetch and activate the new assets.
  onVersionChanged: () => Promise<void>;
};

type VersionFilePayload = {
  buildId?: string;
};

const readRemoteBuildId = async () => {
  // no-store so an HTTP-cached copy cannot mask a new deployment.
  const response = await fetch(VERSION_FILE_URL, { cache: 'no-store' });

  if (!response.ok) {
    throw new Error(`Unexpected status ${response.status}`);
  }

  const payload = (await response.json()) as VersionFilePayload;

  return payload.buildId;
};

const checkForNewVersion = async (
  { clientBuildId, onVersionChanged }: VersionCheckOptions,
  reason: string,
) => {
  // Focus and visibility handlers can run together, so a check is single-flight.
  if (pwaRuntimeState.versionCheckInFlight) {
    logger.info(
      'PWA',
      `Deployed build check (${reason}): skipped, a check is already in flight`,
    );
    return;
  }

  pwaRuntimeState.versionCheckInFlight = true;

  try {
    const remoteBuildId = await readRemoteBuildId();

    // Every completed check logs its outcome so this channel can be watched in DevTools.
    if (!remoteBuildId || remoteBuildId === clientBuildId) {
      logger.info(
        'PWA',
        `Deployed build check (${reason}): up to date (running=${clientBuildId})`,
      );
      return;
    }

    // Only react to a deployed build that has not been handled yet, so a mismatch that cannot be
    // resolved yet (service worker install still in flight) does not re-trigger on every check.
    if (pwaRuntimeState.handledRemoteBuildId === remoteBuildId) {
      logger.info(
        'PWA',
        `Deployed build check (${reason}): deployed build already handled (deployed=${remoteBuildId})`,
      );
      return;
    }

    pwaRuntimeState.handledRemoteBuildId = remoteBuildId;

    logger.info(
      'PWA',
      `Deployed build check (${reason}): deployed build differs, checking for a service worker update (running=${clientBuildId}, deployed=${remoteBuildId})`,
    );

    await onVersionChanged();
  } catch (error) {
    // Offline and transient failures are expected; the next check retries.
    logger.warn('PWA', `Deployed build check failed (${reason})`, error);
  } finally {
    pwaRuntimeState.versionCheckInFlight = false;
  }
};

// Second update-detection channel. The service worker path can only report an update once a
// waiting worker exists, so this compares the deployed build against the running build to catch a
// stale client when that path is degraded (registration errors, failed checks, evicted worker,
// idle tab). It never applies an update itself.
function startVersionChecks(options: VersionCheckOptions) {
  if (!options.clientBuildId) {
    logger.info('PWA', 'Deployed build checks disabled (no client build id)');
    return;
  }

  // Avoid duplicate listeners (possible if initialization path changes in future).
  if (pwaRuntimeState.versionCheckListenersAttached) {
    return;
  }

  pwaRuntimeState.versionCheckListenersAttached = true;

  const runCheck = (reason: string) => {
    void checkForNewVersion(options, reason);
  };

  // Immediate check catches a deployment that happened before this session opened.
  runCheck('startup');

  // Re-check when user returns to the window.
  window.addEventListener('focus', () => {
    runCheck('window-focus');
  });

  // Re-check when tab becomes visible again.
  document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible') {
      runCheck('tab-visible');
    }
  });

  // Periodic checks while visible so long-lived tabs still discover deployments.
  pwaRuntimeState.versionCheckIntervalId = window.setInterval(() => {
    if (document.visibilityState === 'visible') {
      runCheck('interval');
    }
  }, VERSION_CHECK_INTERVAL_MS);
}

export { startVersionChecks };
