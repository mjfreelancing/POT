import { useEffect } from 'react';

import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { useErrorContext } from '@/contexts';
import { logger } from '@/lib/logging';

import { PermissionGuard } from '../auth/components';
import {
  AccountsOverview,
  DashboardHeader,
  ExpensesOverview,
  QuickActions,
} from './components';

function DashboardPage() {
  useEffect(() => {
    logger.info('DashboardPage', 'Mounted');

    return () => {
      logger.info('DashboardPage', 'Unmounted');
    };
  }, []);

  const { error, setError } = useErrorContext();

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20 overflow-x-hidden">
      <DashboardHeader />

      <div className="flex-1 p-6 space-y-6">
        <QuickActions />

        <PermissionGuard permission="expense:view">
          <ExpensesOverview />
        </PermissionGuard>

        <PermissionGuard permission="account:view">
          <AccountsOverview />
        </PermissionGuard>

        {error && (
          <ErrorSheet
            title={error.title}
            description={error.description}
            onDismiss={() => setError(null)}
          />
        )}
      </div>
    </div>
  );
}

export default DashboardPage;
