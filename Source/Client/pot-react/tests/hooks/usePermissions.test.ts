import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import type { Permission } from '@/concerns';
import usePermissions from '@/hooks/usePermissions';
import { useUserStore } from '@/stores';

vi.mock('@/stores', () => ({
  useUserStore: vi.fn(),
}));

type MockUserStoreState = {
  userInfo: {
    permissions: Permission[];
  } | null;
};

describe('usePermissions', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('exposes an empty permission array when userInfo is missing', () => {
    vi.mocked(useUserStore).mockImplementation(selector =>
      selector({ userInfo: null } as MockUserStoreState),
    );

    const { result } = renderHook(() => usePermissions());

    expect(result.current.permissions).toEqual([]);
    expect(result.current.hasPermission('account:view')).toBe(false);
  });

  test('hasPermission returns true only for matching permission', () => {
    vi.mocked(useUserStore).mockImplementation(selector =>
      selector({
        userInfo: {
          permissions: ['account:view', 'expense:manage'],
        },
      } as MockUserStoreState),
    );

    const { result } = renderHook(() => usePermissions());

    expect(result.current.hasPermission('account:view')).toBe(true);
    expect(result.current.hasPermission('income:manage')).toBe(false);
  });

  test('hasAnyPermission returns true when at least one permission matches', () => {
    vi.mocked(useUserStore).mockImplementation(selector =>
      selector({
        userInfo: {
          permissions: ['user:view', 'site:view'],
        },
      } as MockUserStoreState),
    );

    const { result } = renderHook(() => usePermissions());

    expect(result.current.hasAnyPermission(['site:manage', 'site:view'])).toBe(
      true,
    );

    expect(result.current.hasAnyPermission(['income:manage', 'user:manage'])).toBe(
      false,
    );
  });

  test('hasAllPermissions returns true only when every permission matches', () => {
    vi.mocked(useUserStore).mockImplementation(selector =>
      selector({
        userInfo: {
          permissions: ['user:view', 'user:manage', 'site:view'],
        },
      } as MockUserStoreState),
    );

    const { result } = renderHook(() => usePermissions());

    expect(result.current.hasAllPermissions(['user:view', 'user:manage'])).toBe(
      true,
    );

    expect(result.current.hasAllPermissions(['user:view', 'site:manage'])).toBe(
      false,
    );
  });

  test('hasAnyPermission and hasAllPermissions keep empty-array semantics', () => {
    vi.mocked(useUserStore).mockImplementation(selector =>
      selector({
        userInfo: {
          permissions: ['account:view'],
        },
      } as MockUserStoreState),
    );

    const { result } = renderHook(() => usePermissions());

    expect(result.current.hasAnyPermission([])).toBe(false);
    expect(result.current.hasAllPermissions([])).toBe(true);
  });
});
