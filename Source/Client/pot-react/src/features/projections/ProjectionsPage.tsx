import { addDays, addMonths } from 'date-fns';
import { useEffect, useState } from 'react';

import { useApiGetProjection } from '@/api/hooks/useProjections';
import { ErrorSheet, LoadingMessage } from '@/components/feedback';
import { ProjectionMetric } from '@/data/projection';
import { DisplayError } from '@/lib';
import { dateIsoFormat, normalizeToLocalMidnight } from '@/lib/dateUtils';
import { logger } from '@/lib/logging';

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

  const { getProjectionData, setProjectionData } =
    useProjectionStorage(setStorageError);

  // State for start date and period (in months)
  const today = normalizeToLocalMidnight(new Date());
  const [startDate, setStartDate] = useState<Date>(today);

  const [period, setPeriod] = useState<number>(() => {
    const data = getProjectionData();

    return (
      (data && data.period) || (projectionStorageDefaults.period as number)
    );
  });

  const [metric, setMetric] = useState<ProjectionMetric>(() => {
    const data = getProjectionData();

    return (
      (data && data.metric) ||
      (projectionStorageDefaults.metric as ProjectionMetric)
    );
  });

  const [hiddenSeries, setHiddenSeries] = useState<string[]>(() => {
    const data = getProjectionData();
    return data?.hiddenSeries || [];
  });

  // Always fetch 12 months of data from the selected start date
  const apiEndDate = addDays(addMonths(startDate, 12), -1);

  function handleStartDateChange(date?: Date) {
    const newDate = normalizeToLocalMidnight(date ?? today);
    logger.info('ProjectionsPage', 'Start date changed', newDate);
    setStartDate(newDate);
  }

  function handlePeriodChange(months: number) {
    logger.info('ProjectionsPage', 'Projection period changed', months);
    setPeriod(months);

    const existingData = getProjectionData();
    const newData = {
      ...existingData,
      period: months,
    };

    setProjectionData(newData);
  }

  function handleMetricChange(value: ProjectionMetric) {
    logger.info('ProjectionsPage', 'Metric changed', value);
    setMetric(value);

    const existingData = getProjectionData();
    const newData = {
      ...existingData,
      metric: value,
    };

    setProjectionData(newData);
  }

  function handleHiddenSeriesChange(hiddenSeriesKeys: string[]) {
    logger.info('ProjectionsPage', 'Hidden series changed', hiddenSeriesKeys);
    setHiddenSeries(hiddenSeriesKeys);

    const existingData = getProjectionData();
    const newData = {
      ...existingData,
      hiddenSeries: hiddenSeriesKeys,
    };

    setProjectionData(newData);
  }

  // Call API with local date strings for 12 months
  const { data: projectionResult, isLoading } = useApiGetProjection(
    dateIsoFormat(startDate),
    dateIsoFormat(apiEndDate),
  );

  useEffect(() => {
    if (!projectionResult) {
      return;
    }

    if (projectionResult.success) {
      setApiError(null);
    } else {
      setApiError({
        title: projectionResult.error.code,
        description: projectionResult.error.description,
      });
    }
  }, [projectionResult]);

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20">
      <ProjectionsHeader />
      {projectionResult?.success && (
        <div className="p-6 flex-1 min-h-0">
          <ProjectionChart
            data={projectionResult.value}
            startDate={startDate}
            period={period}
            selectedMetric={metric}
            hiddenSeries={hiddenSeries}
            onStartDateChange={handleStartDateChange}
            onPeriodChange={handlePeriodChange}
            onMetricChange={handleMetricChange}
            onHiddenSeriesChange={handleHiddenSeriesChange}
          />
        </div>
      )}

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

      <LoadingMessage isLoading={isLoading} />
    </div>
  );
}

export default ProjectionsPage;
