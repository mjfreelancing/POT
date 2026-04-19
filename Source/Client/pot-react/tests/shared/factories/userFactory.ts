import type { User } from '@/data/user';

function createUser(overrides: Partial<User> = {}): User {
  return {
    rowId: 'user-1',
    etag: 1 as unknown as bigint,
    username: 'maria',
    displayName: 'Maria Carter',
    email: 'maria@example.com',
    permissions: ['accounts:read'],
    site: {
      rowId: 'site-1',
      etag: 1 as unknown as bigint,
      name: 'Main Site',
      description: 'Primary site',
    },
    ...overrides,
  };
}

export { createUser };
