import { addDays, addMonths } from 'date-fns';
import { useCallback, useEffect, useMemo, useState } from 'react';

import { useApiGetProjection } from '@/api/hooks/useProjections';
import { ErrorSheet, LoadingOverlay } from '@/components/feedback';
import { logger } from '@/concerns';
import type { ProjectionMetric } from '@/data/projection';
import type { DisplayError } from '@/lib';
import {
  dateIsoFormat,
  formatDate,
  isAfterDate,
  isSameDate,
  normalizeToLocalMidnight,
} from '@/lib/dateUtils';

import { ProjectionChart, ProjectionsHeader } from './components';
import useProjectionStorage, {
  projectionStorageDefaults,
} from './hooks/useProjectionStorage';

function ProjectionsPage() {
  useEffect(() => {
    logger.info('ProjectionsPage', 'Mounted');

    return () => {
      logger.info('ProjectionsPage', 'Unmounted');
    };
  }, []);

  // Separate error states for API and storage is needed to ensure the success
  // of one does not clear the error shown for the other.
  const [apiError, setApiError] = useState<DisplayError | null>(null);
  const [storageError, setStorageError] = useState<DisplayError | null>(null);

  // Computed property that combines errors for display
  const error = storageError || apiError;

  const {
    getProjectionStorageData,
    setProjectionStorageData,
    removeStorageStartDate,
  } = useProjectionStorage(setStorageError);

  // State for start date and period (in months)
  const today = normalizeToLocalMidnight(new Date());

  // Use the max of stored startDate and today, or today if not present
  const [startDate, setStartDate] = useState<Date>(() => {
    const data = getProjectionStorageData();

    if (data && data.startDate) {
      const stored = normalizeToLocalMidnight(data.startDate);

      if (isAfterDate(stored, today)) {
        return stored;
      }

      // If stored date is before today, clean up local storage
      if (isAfterDate(today, stored)) {
        removeStorageStartDate();
      }
    }

    return today;
  });

  const [period, setPeriod] = useState<number>(() => {
    const data = getProjectionStorageData();

    return (
      (data && data.period) || (projectionStorageDefaults.period as number)
    );
  });

  const [metric, setMetric] = useState<ProjectionMetric>(() => {
    const data = getProjectionStorageData();

    return (
      (data && data.metric) ||
      (projectionStorageDefaults.metric as ProjectionMetric)
    );
  });

  const [hiddenSeries, setHiddenSeries] = useState<string[]>(() => {
    const data = getProjectionStorageData();
    return data?.hiddenSeries || [];
  });

  // State for details panel
  const [isDetailsOpen, setIsDetailsOpen] = useState(false);
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);

  const updateStorage = useCallback(
    <T,>(key: string, value: T) => {
      logger.info('ProjectionsPage', `${key} updated in storage`, value);

      const existingData = getProjectionStorageData();

      setProjectionStorageData({
        ...existingData,
        [key]: value,
      });
    },
    [getProjectionStorageData, setProjectionStorageData],
  );

  const handleStartDateChange = useCallback(
    (date?: Date) => {
      const newDate = normalizeToLocalMidnight(date ?? today);

      logger.info('ProjectionsPage', 'Start date changed', formatDate(newDate));

      setStartDate(newDate);

      // If the new date is today, remove from storage; else, persist
      if (isSameDate(newDate, today)) {
        removeStorageStartDate();
      } else {
        updateStorage('startDate', dateIsoFormat(newDate));
      }
    },
    [today, removeStorageStartDate, updateStorage],
  );

  function handlePeriodChange(months: number) {
    logger.info('ProjectionsPage', 'Projection period changed', months);

    setPeriod(months);
    updateStorage('period', months);
  }

  function handleMetricChange(value: ProjectionMetric) {
    logger.info('ProjectionsPage', 'Metric changed', value);

    setMetric(value);
    updateStorage('metric', value);
  }

  function handleHiddenSeriesChange(hiddenSeriesKeys: string[]) {
    logger.info('ProjectionsPage', 'Hidden series changed', hiddenSeriesKeys);

    setHiddenSeries(hiddenSeriesKeys);
    updateStorage('hiddenSeries', hiddenSeriesKeys);
  }

  // Opens or closes the details panel for a specific date
  function handleToggleDetails(date: Date | null) {
    logger.info(
      'ProjectionsPage',
      'Toggle details',
      date ? formatDate(date) : 'closed',
    );

    if (date) {
      setSelectedDate(date);
      setIsDetailsOpen(true);
    } else {
      setIsDetailsOpen(false);
      // Keep the selected date until the sheet is closed for smooth animation
      setTimeout(() => setSelectedDate(null), 300);
    }
  }

  // This ensures the start date is never before today, which can occur if
  // the user leaves the page open overnight. If it is, update it to today.
  const ensureValidStartDate = useCallback(() => {
    const currentToday = normalizeToLocalMidnight(new Date());

    if (isAfterDate(currentToday, startDate)) {
      logger.info(
        'ProjectionsPage',
        'startDate is earlier than today. Updating startDate.',
        formatDate(currentToday),
      );

      handleStartDateChange(currentToday);

      return currentToday;
    }

    return startDate;
  }, [startDate, handleStartDateChange]);

  // Validate startDate on page mount or re-render to ensure it's not before today
  // which can occur if the user leaves the page open overnight.
  useEffect(() => {
    ensureValidStartDate();
  }, [ensureValidStartDate]);

  // Always fetch 12 months of data from the selected start date
  const apiEndDate = addDays(addMonths(startDate, 12), -1);

  const {
    data: projectionData,
    isLoading: projectionLoading,
    isFetching: projectionFetching,
  } = useApiGetProjection(dateIsoFormat(startDate), dateIsoFormat(apiEndDate));

  const isLoading = useMemo(
    () => projectionLoading || projectionFetching,
    [projectionLoading, projectionFetching],
  );

  useEffect(() => {
    if (!projectionData) {
      return;
    }

    if (projectionData.success) {
      setApiError(null);
    } else {
      setApiError({
        title: projectionData.error.code,
        description: projectionData.error.description,
      });
    }
  }, [projectionData]);

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20">
      <ProjectionsHeader />
      <div className="p-6 flex-1 min-h-0 relative">
        {isLoading && <LoadingOverlay />}
        {projectionData?.success && (
          <ProjectionChart
            data={projectionData.value}
            startDate={startDate}
            period={period}
            selectedMetric={metric}
            hiddenSeries={hiddenSeries}
            onStartDateChange={handleStartDateChange}
            onPeriodChange={handlePeriodChange}
            onMetricChange={handleMetricChange}
            onHiddenSeriesChange={handleHiddenSeriesChange}
            isDetailsOpen={isDetailsOpen}
            selectedDate={selectedDate}
            onToggleDetails={handleToggleDetails}
          />
        )}
      </div>

      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => {
            // Clear the specific error that's being shown
            if (error === apiError) {
              setApiError(null);
            } else {
              setStorageError(null);
            }
          }}
        />
      )}
    </div>
  );
}

export default ProjectionsPage;
