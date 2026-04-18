import { beforeEach, describe, expect, test } from 'vitest';

import type { User } from '@/data/user';
import useUserStore from '@/stores/useUserStore';

const createUser = (overrides?: Partial<User>): User => ({
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
});

describe('useUserStore', () => {
  beforeEach(async () => {
    window.localStorage.clear();

    useUserStore.getState().clearUserInfo();
    await useUserStore.persist.clearStorage();
  });

  test('setUserInfo stores user in state', () => {
    const user = createUser();

    useUserStore.getState().setUserInfo(user);

    expect(useUserStore.getState().userInfo).toEqual(user);
  });

  test('clearUserInfo resets user to null', () => {
    const user = createUser();
    useUserStore.getState().setUserInfo(user);

    useUserStore.getState().clearUserInfo();

    expect(useUserStore.getState().userInfo).toBeNull();
  });

  test('persists only userInfo in localStorage', () => {
    const user = createUser();

    useUserStore.getState().setUserInfo(user);

    const persistedRaw = window.localStorage.getItem('pot-user');

    expect(persistedRaw).not.toBeNull();

    const persisted = JSON.parse(persistedRaw as string) as {
      state: {
        userInfo: User;
      };
      version: number;
    };

    expect(persisted.state.userInfo).toEqual(user);
    expect(Object.keys(persisted.state)).toEqual(['userInfo']);
  });

  test('rehydrates userInfo from persisted localStorage state', async () => {
    const persistedUser = createUser({
      rowId: 'user-2',
      username: 'alex',
      displayName: 'Alex Riley',
      email: 'alex@example.com',
    });

    useUserStore.getState().clearUserInfo();

    window.localStorage.setItem(
      'pot-user',
      JSON.stringify({
        state: {
          userInfo: persistedUser,
        },
        version: 0,
      }),
    );

    await useUserStore.persist.rehydrate();

    expect(useUserStore.getState().userInfo).toEqual(persistedUser);
  });
});
