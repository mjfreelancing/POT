import { renderHook } from '@testing-library/react';
import { createUser } from '@tests/shared/factories/userFactory';
import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

import type { ProjectionMetric } from '@/data/projection';
import { DEFAULT_PROJECTION_INCLUDE } from '@/data/projection';
import useProjectionStorage, {
  projectionStorageDefaults,
} from '@/features/projections/hooks/useProjectionStorage';
import useUserStore from '@/stores/useUserStore';

vi.mock('@/stores/useUserStore', () => ({
  default: vi.fn(),
}));

// In the test environment MODE is 'test', so resolveStorageEnv() returns 'dev'.
const USER_ID = 'user-1';
const SCOPED_KEY = `pot:dev:user:${USER_ID}:projections`;

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
          metric: 'dailyAccrual' as ProjectionMetric,
        });
        result.current.removeStorageStartDate();
      }).not.toThrow();
    });
  });

  describe('session seeding on first read', () => {
    test('seeds sessionStorage from localStorage when sessionStorage has no entry', () => {
      const persistedData = {
        metric: 'dailyAccrual',
        period: 3,
        hiddenSeries: ['account-2'],
      };
      localStorage.setItem(SCOPED_KEY, JSON.stringify(persistedData));

      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData()).toEqual({
        startDate: undefined,
        ...persistedData,
        include: DEFAULT_PROJECTION_INCLUDE,
      });

      expect(JSON.parse(sessionStorage.getItem(SCOPED_KEY)!)).toEqual({
        ...persistedData,
        include: DEFAULT_PROJECTION_INCLUDE,
      });
    });

    test('does not overwrite sessionStorage when it already has an entry', () => {
      const sessionData = { metric: 'balance', period: 12, hiddenSeries: [] };
      const persistedData = {
        metric: 'dailyAccrual',
        period: 3,
        hiddenSeries: ['account-2'],
      };

      sessionStorage.setItem(SCOPED_KEY, JSON.stringify(sessionData));
      localStorage.setItem(SCOPED_KEY, JSON.stringify(persistedData));

      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData()).toEqual({
        startDate: undefined,
        ...sessionData,
        include: DEFAULT_PROJECTION_INCLUDE,
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
        metric: 'dailyAccrual',
        period: 3,
        hiddenSeries: ['account-2'],
      };

      sessionStorage.setItem(SCOPED_KEY, JSON.stringify(sessionData));

      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData()).toEqual({
        ...sessionData,
        include: DEFAULT_PROJECTION_INCLUDE,
      });
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
        JSON.stringify({ metric: 'dailyAccrual' }),
      );

      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData()).toEqual({
        metric: 'dailyAccrual',
        period: projectionStorageDefaults.period,
        hiddenSeries: projectionStorageDefaults.hiddenSeries,
        startDate: undefined,
        include: DEFAULT_PROJECTION_INCLUDE,
      });
    });
  });

  describe('setProjectionStorageData', () => {
    test('writes to both sessionStorage and localStorage', () => {
      const data = {
        startDate: '2026-06-01',
        metric: 'dailyAccrual' as ProjectionMetric,
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
        metric: 'dailyAccrual' as ProjectionMetric,
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
        metric: 'dailyAccrual',
        period: 3,
        hiddenSeries: ['account-2'],
      };

      sessionStorage.setItem(SCOPED_KEY, JSON.stringify(data));
      localStorage.setItem(SCOPED_KEY, JSON.stringify(data));

      const { result } = renderHook(() => useProjectionStorage());

      result.current.removeStorageStartDate();

      const expectedWithoutStartDate = {
        metric: 'dailyAccrual',
        period: 3,
        hiddenSeries: ['account-2'],
        include: DEFAULT_PROJECTION_INCLUDE,
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

  describe('validation', () => {
    test('resolves an unknown stored metric to the default metric and switches', () => {
      sessionStorage.setItem(
        SCOPED_KEY,
        JSON.stringify({ metric: 'available', period: 3, hiddenSeries: [] }),
      );

      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData()).toEqual({
        startDate: undefined,
        metric: projectionStorageDefaults.metric,
        period: 3,
        hiddenSeries: [],
        include: DEFAULT_PROJECTION_INCLUDE,
      });
    });

    test('seeds session storage with the validated record when local storage holds an unknown metric', () => {
      localStorage.setItem(
        SCOPED_KEY,
        JSON.stringify({ metric: 'available', period: 3, hiddenSeries: [] }),
      );

      const { result } = renderHook(() => useProjectionStorage());

      const expected = {
        startDate: undefined,
        metric: projectionStorageDefaults.metric,
        period: 3,
        hiddenSeries: [],
        include: DEFAULT_PROJECTION_INCLUDE,
      };

      expect(result.current.getProjectionStorageData()).toEqual(expected);
      expect(JSON.parse(sessionStorage.getItem(SCOPED_KEY)!)).toEqual(expected);
    });

    test('a write after an unknown stored metric leaves neither store holding it', () => {
      const legacyData = {
        metric: 'available',
        period: 3,
        hiddenSeries: [],
      };
      sessionStorage.setItem(SCOPED_KEY, JSON.stringify(legacyData));
      localStorage.setItem(SCOPED_KEY, JSON.stringify(legacyData));

      const { result } = renderHook(() => useProjectionStorage());

      const validatedData = result.current.getProjectionStorageData();

      result.current.setProjectionStorageData({ ...validatedData, period: 9 });

      const expected = {
        startDate: undefined,
        metric: projectionStorageDefaults.metric,
        period: 9,
        hiddenSeries: [],
        include: DEFAULT_PROJECTION_INCLUDE,
      };

      expect(JSON.parse(sessionStorage.getItem(SCOPED_KEY)!)).toEqual(expected);
      expect(JSON.parse(localStorage.getItem(SCOPED_KEY)!)).toEqual(expected);
      expect(sessionStorage.getItem(SCOPED_KEY)).not.toContain('available');
      expect(localStorage.getItem(SCOPED_KEY)).not.toContain('available');
    });

    test('fills a missing include from the defaults', () => {
      sessionStorage.setItem(
        SCOPED_KEY,
        JSON.stringify({ metric: 'balance', period: 6, hiddenSeries: [] }),
      );

      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData().include).toEqual(
        DEFAULT_PROJECTION_INCLUDE,
      );
    });

    test('falls back per member when an include member is not a boolean', () => {
      sessionStorage.setItem(
        SCOPED_KEY,
        JSON.stringify({
          metric: 'balance',
          period: 6,
          hiddenSeries: [],
          include: { reserved: true, accruals: 'yes', arrears: false },
        }),
      );

      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData().include).toEqual({
        reserved: true,
        accruals: DEFAULT_PROJECTION_INCLUDE.accruals,
        arrears: false,
      });
    });

    test('returns valid include members unchanged', () => {
      const include = { reserved: true, accruals: false, arrears: true };

      sessionStorage.setItem(
        SCOPED_KEY,
        JSON.stringify({
          metric: 'balance',
          period: 6,
          hiddenSeries: [],
          include,
        }),
      );

      const { result } = renderHook(() => useProjectionStorage());

      expect(result.current.getProjectionStorageData().include).toEqual(
        include,
      );
    });
  });
});
