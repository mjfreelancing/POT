import { useApiGetProjection } from '@/api/hooks/useProjections';
import { ErrorSheet, LoadingMessage } from '@/components/feedback';
import { DisplayError } from '@/lib';
import { useEffect, useState } from 'react';

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
    <>
      <LoadingMessage isLoading={isLoading} />

      <div className="flex h-full w-full items-center justify-center">
        Projections Content Here
      </div>

      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
    </>
  );
}

export default ProjectionsPage;
