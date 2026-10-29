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

// Dedicated identity for the dashboard quick-actions suite. It is an Admin on
// its OWN site (see baseline.sql) so the suite's whole-site renew actions never
// touch the shared E2E site's data.
const quickActionsCredentials = {
  username: 'e2e_quickactions',
  password: 'E2E_quickactions-password',
} as const;

// The suites that destroy a tenant (account and site deletion) provision their own
// identity through the product's routes rather than sharing a seeded one, so the
// identities below are only ever read.

export {
  adminCredentials,
  pwChangeCredentials,
  quickActionsCredentials,
  viewerCredentials,
};

