import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, test, vi } from 'vitest';

import useAccountStorage from '@/features/accounts/hooks/useAccountStorage';
import useLocalStorageManager from '@/hooks/useLocalStorageManager';
import useUserStore from '@/stores/useUserStore';

vi.mock('@/hooks/useLocalStorageManager', () => ({
  default: vi.fn(),
}));

vi.mock('@/stores/useUserStore', () => ({
  default: vi.fn(),
}));

function mockUserStore(userId: string | undefined) {
  vi.mocked(useUserStore).mockImplementation(selector =>
    selector({
      userInfo: userId ? { rowId: userId } : null,
    } as Parameters<typeof selector>[0]),
  );
}

describe('useAccountStorage', () => {
  beforeEach(() => {
    sessionStorage.clear();
    localStorage.clear();
    mockUserStore('user-1');
    vi.clearAllMocks();
  });

  test('wires scoped key and sessionStorage backend into useLocalStorageManager', () => {
    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    const onError = vi.fn();
    renderHook(() => useAccountStorage(onError));

    expect(useLocalStorageManager).toHaveBeenCalledWith(
      'pot:dev:user:user-1:accounts',
      onError,
      { filterDescription: null },
      sessionStorage,
    );
  });

  test('purges legacy accounts key from localStorage when user is authenticated', () => {
    localStorage.setItem(
      'pot-accounts',
      JSON.stringify({ filterDescription: 'legacy' }),
    );

    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    renderHook(() => useAccountStorage());

    expect(localStorage.getItem('pot-accounts')).toBeNull();
  });

  test('setAccountData converts empty filter string to null', () => {
    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    const { result } = renderHook(() => useAccountStorage());

    result.current.setAccountData({ filterDescription: '' });

    expect(setProperty).toHaveBeenCalledWith('filterDescription', null);
  });

  test('setAccountData is a no-op when userId is unavailable', () => {
    mockUserStore(undefined);

    const getProperty = vi.fn();
    const setProperty = vi.fn();

    vi.mocked(useLocalStorageManager).mockReturnValue({
      getProperty,
      setProperty,
    } as unknown as ReturnType<typeof useLocalStorageManager>);

    const { result } = renderHook(() => useAccountStorage());

    result.current.setAccountData({ filterDescription: 'rent' });

    expect(setProperty).not.toHaveBeenCalled();
  });
});
