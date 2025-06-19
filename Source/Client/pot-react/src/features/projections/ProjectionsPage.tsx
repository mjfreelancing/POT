import { useEffect, useState } from 'react';

import { useApiGetProjection } from '@/api/hooks/useProjections';
import { ErrorSheet, LoadingMessage } from '@/components/feedback';
import { DisplayError } from '@/lib';

import { NoProjectionData, ProjectionChart } from './components';

function ProjectionsPage() {
  const { data: result, isLoading } = useApiGetProjection();
  const [error, setError] = useState<DisplayError | null>(null);

  useEffect(() => {
    // Transient error handling - resetting error state when success, such as after a network loss
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
          <ProjectionChart data={result.value} />
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
