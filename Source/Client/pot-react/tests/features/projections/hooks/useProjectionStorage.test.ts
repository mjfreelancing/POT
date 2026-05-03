import { renderHook } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

import useProjectionStorage, {
  projectionStorageDefaults,
} from '@/features/projections/hooks/useProjectionStorage';
import useUserStore from '@/stores/useUserStore';

import type { ProjectionMetric } from '@/data/projection';
import { createUser } from '@tests/shared/factories/userFactory';

vi.mock('@/stores/useUserStore', () => ({
  default: vi.fn(),
}));

// In the test environment MODE is 'test', so resolveStorageEnv() returns 'dev'.
const USER_ID = 'user-1';
const SCOPED_KEY = `pot:dev:user:${USER_ID}:projections`;
const LEGACY_KEY = 'pot-projections';

function mockUserStore(userId: string | undefined) {
  const user = userId ? createUser({ rowId: userId }) : null;

  vi.mocked(useUserStore).mockImplementation(selector =>
    selector({ userInfo: user } as Parameters<typeof selector>[0]),
  );
}

describe('useProjectionStorage', () => {
  beforeEach(() => {
    mockUserStore(USER_ID);
    localStorage.clear();
    sessionStorage.clear();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('authentication transition', () => {
    test('returns defaults and no-op handlers when userId is unavailable', () => {
      mockUserStore(undefined);

      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData()).toEqual(
        projectionStorageDefaults,
      );

      expect(() => {
        result.current.setProjectionStorageData({
          metric: 'available' as ProjectionMetric,
        });
        result.current.removeStorageStartDate();
      }).not.toThrow();
    });
  });

  describe('legacy key cleanup', () => {
    test('removes the legacy pot-projections key from localStorage on mount', () => {
      localStorage.setItem(LEGACY_KEY, JSON.stringify({ period: 3 }));

      renderHook(() => useProjectionStorage());

      expect(localStorage.getItem(LEGACY_KEY)).toBeNull();
    });
  });

  describe('session seeding on first read', () => {
    test('seeds sessionStorage from localStorage when sessionStorage has no entry', () => {
      const persistedData = {
        metric: 'available',
        period: 3,
        hiddenSeries: ['account-2'],
      };
      localStorage.setItem(SCOPED_KEY, JSON.stringify(persistedData));

      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData()).toEqual({
        startDate: undefined,
        ...persistedData,
      });

      expect(JSON.parse(sessionStorage.getItem(SCOPED_KEY)!)).toEqual(
        persistedData,
      );
    });

    test('does not overwrite sessionStorage when it already has an entry', () => {
      const sessionData = { metric: 'balance', period: 12, hiddenSeries: [] };
      const persistedData = {
        metric: 'available',
        period: 3,
        hiddenSeries: ['account-2'],
      };

      sessionStorage.setItem(SCOPED_KEY, JSON.stringify(sessionData));
      localStorage.setItem(SCOPED_KEY, JSON.stringify(persistedData));

      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData()).toEqual({
        startDate: undefined,
        ...sessionData,
      });

      expect(JSON.parse(sessionStorage.getItem(SCOPED_KEY)!)).toEqual(
        sessionData,
      );
    });

    test('does not seed sessionStorage when localStorage also has no entry and returns defaults', () => {
      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData()).toEqual(
        projectionStorageDefaults,
      );

      expect(sessionStorage.getItem(SCOPED_KEY)).toBeNull();
    });
  });

  describe('getProjectionStorageData', () => {
    test('returns data from sessionStorage', () => {
      const sessionData = {
        startDate: '2026-05-01',
        metric: 'available',
        period: 3,
        hiddenSeries: ['account-2'],
      };

      sessionStorage.setItem(SCOPED_KEY, JSON.stringify(sessionData));

      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData()).toEqual(sessionData);
    });

    test('returns defaults for fields absent from sessionStorage', () => {
      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData()).toEqual(
        projectionStorageDefaults,
      );
    });

    test('applies individual defaults for each missing field', () => {
      sessionStorage.setItem(
        SCOPED_KEY,
        JSON.stringify({ metric: 'available' }),
      );

      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData()).toEqual({
        metric: 'available',
        period: projectionStorageDefaults.period,
        hiddenSeries: projectionStorageDefaults.hiddenSeries,
        startDate: undefined,
      });
    });
  });

  describe('setProjectionStorageData', () => {
    test('writes to both sessionStorage and localStorage', () => {
      const data = {
        startDate: '2026-06-01',
        metric: 'available' as ProjectionMetric,
        period: 12,
        hiddenSeries: ['account-1'],
      };

      const { result } = renderHook(() => useProjectionStorage());

      result.current.setProjectionStorageData(data);

      expect(JSON.parse(sessionStorage.getItem(SCOPED_KEY)!)).toEqual(data);
      expect(JSON.parse(localStorage.getItem(SCOPED_KEY)!)).toEqual(data);
    });

    test('writing in one rendered instance does not affect another independent instance', () => {
      const tabAData = {
        metric: 'balance' as ProjectionMetric,
        period: 6,
        hiddenSeries: [],
      };
      const tabBData = {
        startDate: '2026-06-01',
        metric: 'available' as ProjectionMetric,
        period: 3,
        hiddenSeries: ['account-1'],
      };

      sessionStorage.setItem(SCOPED_KEY, JSON.stringify(tabAData));

      const { result } = renderHook(() => useProjectionStorage());

      result.current.setProjectionStorageData(tabBData);

      expect(JSON.parse(localStorage.getItem(SCOPED_KEY)!)).toEqual(tabBData);
      expect(JSON.parse(sessionStorage.getItem(SCOPED_KEY)!)).toEqual(tabBData);
    });
  });

  describe('removeStorageStartDate', () => {
    test('removes startDate from both sessionStorage and localStorage', () => {
      const data = {
        startDate: '2026-04-01',
        metric: 'available',
        period: 3,
        hiddenSeries: ['account-2'],
      };

      sessionStorage.setItem(SCOPED_KEY, JSON.stringify(data));
      localStorage.setItem(SCOPED_KEY, JSON.stringify(data));

      const { result } = renderHook(() => useProjectionStorage());

      result.current.removeStorageStartDate();

      const expectedWithoutStartDate = {
        metric: 'available',
        period: 3,
        hiddenSeries: ['account-2'],
      };

      expect(JSON.parse(sessionStorage.getItem(SCOPED_KEY)!)).toEqual(
        expectedWithoutStartDate,
      );
      expect(JSON.parse(localStorage.getItem(SCOPED_KEY)!)).toEqual(
        expectedWithoutStartDate,
      );
    });

    test('does not write when startDate does not exist in either store', () => {
      const { result } = renderHook(() => useProjectionStorage());

      result.current.removeStorageStartDate();

      expect(sessionStorage.getItem(SCOPED_KEY)).toBeNull();
      expect(localStorage.getItem(SCOPED_KEY)).toBeNull();
    });
  });
});
