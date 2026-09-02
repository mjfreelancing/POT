/**
 * Canonical E2E credentials.
 *
 * Test-only identities whose passwords must match the hashes committed in
 * `e2e/seed/baseline.sql` (site + canonical users). Centralised here — instead of
 * being duplicated across fixtures, global setup, and tests — for DRY, not
 * security: these are not real secrets.
 */
const adminCredentials = {
  username: 'e2e_admin',
  password: 'E2E_admin-password',
} as const;

const viewerCredentials = {
  username: 'e2e_viewer',
  password: 'E2E_viewer-password',
} as const;

// Dedicated identity for the password-change E2E test.
// It is seeded purely so that the password-change test can mutate
// its own credentials without invalidating the shared fixture admin.
const pwChangeCredentials = {
  username: 'e2e_pwchange',
  password: 'E2E_pwchange-password',
} as const;

export { adminCredentials, pwChangeCredentials, viewerCredentials };
