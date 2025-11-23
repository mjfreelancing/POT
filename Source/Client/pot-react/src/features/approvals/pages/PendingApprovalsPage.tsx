import { useEffect, useMemo } from 'react';

import { useGetPendingApprovals } from '@/api/hooks/useApprovals';
import { ErrorSheet, LoadingOverlay } from '@/components/feedback';
import { PageHeader } from '@/components/layout';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import { usePermissions } from '@/hooks';

import { PendingApprovalsTable } from '../components/PendingApprovalsTable';

function PendingApprovalsPage() {
  const { hasAnyPermission } = usePermissions();
  const { error, setError } = useErrorContext();

  const {
    data: approvalsData,
    isLoading: approvalsLoading,
    isFetching: approvalsFetching,
  } = useGetPendingApprovals();

  const isLoading = useMemo(
    () => approvalsLoading || approvalsFetching,
    [approvalsLoading, approvalsFetching],
  );

  useEffect(() => {
    logger.info('PendingApprovalsPage', 'Mounted');

    return () => {
      logger.info('PendingApprovalsPage', 'Unmounted');
    };
  }, []);

  // Check permissions
  const canManagePlatform = hasAnyPermission(['platform:manage']);

  // Extract pending approvals array from response
  const pendingApprovals = approvalsData?.success ? approvalsData.value : [];

  // Handle errors
  useEffect(() => {
    if (approvalsData) {
      setError(
        approvalsData.success
          ? null
          : {
              title: approvalsData.error.code,
              description: approvalsData.error.description,
            },
      );
    }
  }, [approvalsData, setError]);

  // Only show permission denial when not loading and no error
  if (!canManagePlatform && !isLoading && !error) {
    return <div>You do not have permission to manage platform approvals.</div>;
  }

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20">
      <PageHeader
        title="Pending Approvals"
        subtitle="Review and approve user signups"
      />
      <div className="flex-1 min-h-0 flex flex-col p-6 gap-4">
        <div className="flex-1 min-h-0 flex flex-col relative">
          {isLoading && <LoadingOverlay />}
          <PendingApprovalsTable users={pendingApprovals} />
        </div>
      </div>

      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
    </div>
  );
}

export default PendingApprovalsPage;
