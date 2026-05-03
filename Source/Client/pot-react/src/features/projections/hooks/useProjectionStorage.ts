import {
  buildEnvScopedKey,
  buildUserScopedKey,
  purgeLegacyStorageKeys,
} from '@/concerns/storage';
import type { ProjectionMetric } from '@/data/projection';
import {
  DEFAULT_PROJECTION_METRIC,
  DEFAULT_PROJECTION_PERIOD,
} from '@/data/projection';
import useLocalStorage from '@/hooks/useLocalStorage';
import type { DisplayError } from '@/lib';
import useUserStore from '@/stores/useUserStore';

type ProjectionStorageData = {
  startDate?: string;
  metric?: ProjectionMetric;
  period?: number;
  hiddenSeries?: string[];
};

const projectionStorageDefaults: ProjectionStorageData = {
  metric: DEFAULT_PROJECTION_METRIC,
  period: DEFAULT_PROJECTION_PERIOD,
  hiddenSeries: [],
};

type StorageErrorHandler = (error: DisplayError) => void;

/*
 * Projection storage uses a hybrid strategy to support both tab isolation and
 * sensible seeding for new tabs/sessions.
 *
 * Read order:
 *   1. sessionStorage
 *   2. localStorage
 *   3. defaults
 *
 * This allows already-open tabs to maintain independent working state while a
 * fresh tab can still start from the user's last-used projection settings.
 * When localStorage seeds a fresh tab, the data is copied into sessionStorage so
 * the new tab becomes independent from that point forward.
 */
function useProjectionStorage(onError?: StorageErrorHandler) {
  const userId = useUserStore(store => store.userInfo?.rowId);
  const scopedKey = userId
    ? buildUserScopedKey({ userId, feature: 'projections' })
    : buildEnvScopedKey('unauthenticated:projections');

  // TEMPORARY: remove the legacy flat key now that the user is known and scoped keys are active.
  if (userId) {
    purgeLegacyStorageKeys([{ key: 'pot-projections', storage: localStorage }]);
  }

  // sessionStorage: primary read/write target for tab-local state.
  const { getItem: getSession, setItem: setSession } =
    useLocalStorage<ProjectionStorageData>({
      key: scopedKey,
      onError,
      storage: sessionStorage,
    });

  // localStorage: seed/mirror store for new tabs and fresh sessions.
  const { getItem: getPersistent, setItem: setPersistent } =
    useLocalStorage<ProjectionStorageData>({
      key: scopedKey,
      onError,
      storage: localStorage,
    });

  const getProjectionStorageData = (): ProjectionStorageData => {
    if (!userId) {
      return {
        ...projectionStorageDefaults,
      };
    }

    const sessionData = getSession();

    if (sessionData) {
      return {
        startDate: sessionData.startDate,
        metric: sessionData.metric ?? projectionStorageDefaults.metric,
        period: sessionData.period ?? projectionStorageDefaults.period,
        hiddenSeries:
          sessionData.hiddenSeries ?? projectionStorageDefaults.hiddenSeries,
      };
    }

    const persistentData = getPersistent();

    if (persistentData) {
      setSession(persistentData);

      return {
        startDate: persistentData.startDate,
        metric: persistentData.metric ?? projectionStorageDefaults.metric,
        period: persistentData.period ?? projectionStorageDefaults.period,
        hiddenSeries:
          persistentData.hiddenSeries ?? projectionStorageDefaults.hiddenSeries,
      };
    }

    return {
      ...projectionStorageDefaults,
    };
  };

  const setProjectionStorageData = (data: ProjectionStorageData) => {
    if (!userId) {
      return;
    }

    setSession(data);
    setPersistent(data);
  };

  const removeStorageStartDate = () => {
    if (!userId) {
      return;
    }

    const current = getSession() ?? getPersistent();

    if (!current?.startDate) {
      return;
    }

    const { startDate: _, ...withoutStartDate } = current;

    setSession(withoutStartDate);
    setPersistent(withoutStartDate);
  };

  return {
    getProjectionStorageData,
    setProjectionStorageData,
    removeStorageStartDate,
  };
}

export default useProjectionStorage;
export { projectionStorageDefaults };
export type { ProjectionStorageData, StorageErrorHandler };
