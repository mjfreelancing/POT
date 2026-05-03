import { beforeEach, describe, expect, test, vi } from 'vitest';

describe('useUserStore legacy storage cleanup', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.resetModules();
  });

  test('purges legacy pot-user key during module initialization', async () => {
    localStorage.setItem(
      'pot-user',
      JSON.stringify({
        state: { userInfo: { rowId: 'legacy-user' } },
        version: 0,
      }),
    );

    await import('@/stores/useUserStore');

    expect(localStorage.getItem('pot-user')).toBeNull();
  });
});
