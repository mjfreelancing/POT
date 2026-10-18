import { buildEnvScopedKey } from '@/concerns/storage';

const storageKey = buildEnvScopedKey('session-notice');

/**
 * Records a message to show once on the login page for the session that just ended.
 *
 * The login page is reached by more than one route (this app's own redirect and the protected-route
 * redirect that follows a cleared session), so the message is held in session storage rather than in
 * navigation state that a later redirect would discard.
 */
function setSessionNotice(message: string): void {
  sessionStorage.setItem(storageKey, message);
}

/** Reads the recorded message and clears it, so it is shown once. */
function takeSessionNotice(): string | undefined {
  const message = sessionStorage.getItem(storageKey);

  if (message !== null) {
    sessionStorage.removeItem(storageKey);
  }

  return message ?? undefined;
}

export { setSessionNotice, takeSessionNotice };
