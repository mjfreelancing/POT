import { buildEnvScopedKey, buildUserScopedKey } from '@/concerns/storage';
import type { ProjectionInclude, ProjectionMetric } from '@/data/projection';
import {
  DEFAULT_PROJECTION_INCLUDE,
  DEFAULT_PROJECTION_METRIC,
  DEFAULT_PROJECTION_PERIOD,
  PROJECTION_METRICS,
} from '@/data/projection';
import useLocalStorage from '@/hooks/useLocalStorage';
import type { DisplayError } from '@/lib';
import useUserStore from '@/stores/useUserStore';

type ProjectionStorageData = {
  startDate?: string;
  metric?: ProjectionMetric;
  period?: number;
  hiddenSeries?: string[];
  include?: ProjectionInclude;
};

const projectionStorageDefaults: ProjectionStorageData = {
  metric: DEFAULT_PROJECTION_METRIC,
  period: DEFAULT_PROJECTION_PERIOD,
  hiddenSeries: [],
  include: DEFAULT_PROJECTION_INCLUDE,
};

// A stored metric is only honoured while it is still a current metric key, so a
// retired value such as 'available' resolves to the default metric.
function isProjectionMetric(value: unknown): value is ProjectionMetric {
  return (
    typeof value === 'string' &&
    Object.prototype.hasOwnProperty.call(PROJECTION_METRICS, value)
  );
}

// Validation is per field and always falls back to the default rather than
// migrating: a missing include, or any member that is not a boolean, resolves to
// that member's default (off/off/on). Reading through this means the next write
// of any setting replaces whatever a store held with valid values.
function validateInclude(value: unknown): ProjectionInclude {
  const stored = (value ?? {}) as Partial<
    Record<keyof ProjectionInclude, unknown>
  >;

  return {
    reserved:
      typeof stored.reserved === 'boolean'
        ? stored.reserved
        : DEFAULT_PROJECTION_INCLUDE.reserved,
    accruals:
      typeof stored.accruals === 'boolean'
        ? stored.accruals
        : DEFAULT_PROJECTION_INCLUDE.accruals,
    arrears:
      typeof stored.arrears === 'boolean'
        ? stored.arrears
        : DEFAULT_PROJECTION_INCLUDE.arrears,
  };
}

function validateProjectionStorageData(
  data: ProjectionStorageData,
): ProjectionStorageData {
  return {
    startDate: data.startDate,
    metric: isProjectionMetric(data.metric)
      ? data.metric
      : projectionStorageDefaults.metric,
    period: data.period ?? projectionStorageDefaults.period,
    hiddenSeries: data.hiddenSeries ?? projectionStorageDefaults.hiddenSeries,
    include: validateInclude(data.include),
  };
}

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
      return validateProjectionStorageData(sessionData);
    }

    const persistentData = getPersistent();

    if (persistentData) {
      const validatedData = validateProjectionStorageData(persistentData);

      setSession(validatedData);

      return validatedData;
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

    const { startDate: _, ...withoutStartDate } =
      validateProjectionStorageData(current);

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
