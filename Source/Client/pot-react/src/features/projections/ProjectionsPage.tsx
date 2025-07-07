import { addDays, addMonths } from 'date-fns';
import { useEffect, useState } from 'react';

import { useApiGetProjection } from '@/api/hooks/useProjections';
import { ErrorSheet, LoadingMessage } from '@/components/feedback';
import { DisplayError } from '@/lib';
import { dateIsoFormat, normalizeToLocalMidnight } from '@/lib/dateUtils';

import { NoProjectionData, ProjectionChart } from './components';

function ProjectionsPage() {
  // State for start date and period (in months)
  const today = normalizeToLocalMidnight(new Date());
  const [startDate, setStartDate] = useState<Date>(today);
  const [period, setPeriod] = useState<number>(6); // default 6 months

  // Always fetch 12 months of data from the selected start date
  const apiEndDate = addDays(addMonths(startDate, 12), -1);

  // Handlers
  function handleStartDateChange(date?: Date) {
    setStartDate(normalizeToLocalMidnight(date ?? today));
  }

  function handlePeriodChange(months: number) {
    setPeriod(months);
  }

  // Call API with local date strings for 12 months
  const { data: result, isLoading } = useApiGetProjection(
    dateIsoFormat(startDate),
    dateIsoFormat(apiEndDate),
  );

  const [error, setError] = useState<DisplayError | null>(null);

  useEffect(() => {
    if (result) {
      setError(
        result.success
          ? null
          : {
              title: result.error.code,
              description: result.error.description,
            },
      );
    }
  }, [result]);

  return (
    <div className="flex flex-col h-full bg-gradient-to-br from-background to-muted/20 overflow-hidden">
      {result?.success && (
        <div className="p-6 flex-1 min-h-0">
          <ProjectionChart
            data={result.value}
            startDate={startDate}
            period={period}
            onStartDateChange={handleStartDateChange}
            onPeriodChange={handlePeriodChange}
          />
        </div>
      )}

      {!result?.success && <NoProjectionData />}

      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}

      <LoadingMessage isLoading={isLoading} />
    </div>
  );
}

export default ProjectionsPage;
