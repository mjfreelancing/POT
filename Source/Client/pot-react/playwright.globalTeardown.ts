import { rmSync } from 'node:fs';

import { stopMailSink } from './e2e/helpers/mailSink';
import { runtimeStatePath } from './playwright.globalSetup';

/**
 * Global teardown runs after all tests complete.
 *
 * Playwright resolves a global hook from the module's default export only, so this must live in its own module: a
 * named `globalTeardown` export alongside a default `globalSetup` export is ignored, and the setup then runs a second
 * time as the "teardown" (migrations, seed, import, and a fresh mail sink left behind).
 *
 * We do NOT call `docker stop` here. On Docker Desktop for Windows, stopping a container
 * explicitly during teardown leaves the port in TCP TIME_WAIT. Docker Desktop's proxy layer
 * then reports the port as "already allocated" on the very next test run.
 *
 * Instead we rely on the Testcontainers Ryuk reaper, which removes the container AFTER
 * the Playwright process exits. By the time the user starts another run, the port has been
 * cleanly released.
 *
 * For runs where Ryuk also cannot fire (hard kill / SIGKILL), globalSetup detects the
 * leftover container from the runtime state file at the start of the next run and removes
 * it with `docker rm -f` (see stale cleanup block in the setup module's default export).
 *
 * Otherwise this function stops the run's mail sink and deletes the runtime state file, so a stale container ID
 * written by this run cannot confuse the next run's stale-cleanup logic.
 */
export default async function globalTeardown() {
  // The sink belongs to the run, and the next run must start with an empty inbox, so it is removed here rather
  // than left for the next setup to discover.
  stopMailSink();

  rmSync(runtimeStatePath, { force: true });
  console.log('✅ Runtime state cleared');
}
