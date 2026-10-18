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

// Dedicated identity for the account-deletion suite, which deletes it. It is a
// Viewer on the shared E2E site and reuses the e2e_pwchange password hash (see
// baseline.sql), so it shares that password.
const selfDeleteCredentials = {
  username: 'e2e_selfdelete',
  password: 'E2E_pwchange-password',
} as const;

// Dedicated identity for the site-deletion suite, which deletes its own site. It
// is an Admin on that site and reuses the e2e_pwchange password hash (see
// baseline.sql), so it shares that password.
const siteDeleteCredentials = {
  username: 'e2e_sitedelete',
  password: 'E2E_pwchange-password',
} as const;

// The site owned by siteDeleteCredentials, which the site-deletion suite retires.
const siteDeleteSiteName = "e2e_sitedelete's Site";

export {
  adminCredentials,
  pwChangeCredentials,
  quickActionsCredentials,
  selfDeleteCredentials,
  siteDeleteCredentials,
  siteDeleteSiteName,
  viewerCredentials,
};
