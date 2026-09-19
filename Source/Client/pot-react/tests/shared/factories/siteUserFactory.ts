import type { SiteUser } from '@/data/siteUser';

function createSiteUser(overrides: Partial<SiteUser> = {}): SiteUser {
  return {
    rowId: '11111111-1111-1111-1111-111111111111',
    etag: 1n,
    username: 'maria',
    displayName: 'Maria Carter',
    email: 'maria@example.com',
    roles: ['Admin'],
    status: 'Enabled',
    lastLoggedInUtc: null,
    ...overrides,
  };
}

export { createSiteUser };
