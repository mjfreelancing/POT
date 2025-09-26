import {
  DEFAULT_PROJECTION_METRIC,
  DEFAULT_PROJECTION_PERIOD,
  ProjectionMetric,
} from '@/data/projection';
import useLocalStorage from '@/hooks/useLocalStorage';
import { DisplayError } from '@/lib';

type ProjectionStorageData = {
  startDate?: string;
  metric?: ProjectionMetric;
  period?: number;
  hiddenSeries?: string[];
};

// Default values for projections
const projectionStorageDefaults: ProjectionStorageData = {
  metric: DEFAULT_PROJECTION_METRIC,
  period: DEFAULT_PROJECTION_PERIOD,
  hiddenSeries: [], // Empty array; all series are visible by default
};

// Storage key for projections
const PROJECTION_STORAGE_KEY = 'pot-projections';

type StorageErrorHandler = (error: DisplayError) => void;

/**
 * Custom hook for accessing projection storage
 * @param onError Optional error handler function to report storage errors
 * @returns Storage utilities for projection data
 */
function useProjectionStorage(onError?: StorageErrorHandler) {
  const { getItem, setItem } = useLocalStorage<ProjectionStorageData>({
    key: PROJECTION_STORAGE_KEY,
    defaultValue: projectionStorageDefaults,
    onError: onError,
  });

  return {
    getProjectionStorageData: getItem,
    setProjectionStorageData: setItem,
    removeStorageStartDate: () => {
      const data = getItem();

      if (data && data.startDate) {
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        const { startDate, ...rest } = data; // Intentionally omit startDate from storage

        setItem(rest);
      }
    },
  };
}

export default useProjectionStorage;
export { projectionStorageDefaults };
export type { ProjectionStorageData, StorageErrorHandler };
