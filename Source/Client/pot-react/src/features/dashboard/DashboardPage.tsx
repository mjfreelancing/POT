import { useEffect, useState } from 'react';

import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';

import { PermissionGuard } from '../auth/components';
import {
  AccountsOverview,
  DashboardHeader,
  ExpensesOverview,
  IncomesOverview,
  QuickActions,
} from './components';
import useDashboardStorage from './hooks/useDashboardStorage';

function DashboardPage() {
  useEffect(() => {
    logger.info('DashboardPage', 'Mounted');

    return () => {
      logger.info('DashboardPage', 'Unmounted');
    };
  }, []);

  const { error, setError } = useErrorContext();
  const { getDashboardData, setDashboardData } = useDashboardStorage(setError);

  // Initialize state from localStorage (using lazy initializer to avoid calling on every render)
  const [dashboardState, setDashboardState] = useState(() =>
    getDashboardData(),
  );

  const handleQuickActionsOpenChange = (isOpen: boolean) => {
    setDashboardState(prev => ({ ...prev, quickActionsOpen: isOpen }));
    setDashboardData({ quickActionsOpen: isOpen });
  };

  const handleAccountsOpenChange = (isOpen: boolean) => {
    setDashboardState(prev => ({ ...prev, accountsOpen: isOpen }));
    setDashboardData({ accountsOpen: isOpen });
  };

  const handleIncomesOpenChange = (isOpen: boolean) => {
    setDashboardState(prev => ({ ...prev, incomesOpen: isOpen }));
    setDashboardData({ incomesOpen: isOpen });
  };

  const handleExpensesOpenChange = (isOpen: boolean) => {
    setDashboardState(prev => ({ ...prev, expensesOpen: isOpen }));
    setDashboardData({ expensesOpen: isOpen });
  };

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20">
      <DashboardHeader />

      <div className="flex-1 min-h-0 p-3 sm:p-4 overflow-auto">
        <div className="max-w-7xl mx-auto space-y-6">
          <PermissionGuard
            permissions={['account:manage', 'expense:manage', 'income:manage']}
            mode="any"
          >
            <QuickActions
              isOpen={dashboardState.quickActionsOpen}
              onOpenChange={handleQuickActionsOpenChange}
            />
          </PermissionGuard>

          <PermissionGuard permissions={['account:view']} mode="all">
            <AccountsOverview
              isOpen={dashboardState.accountsOpen}
              onOpenChange={handleAccountsOpenChange}
            />
          </PermissionGuard>

          <PermissionGuard permissions={['expense:view']} mode="all">
            <ExpensesOverview
              isOpen={dashboardState.expensesOpen}
              onOpenChange={handleExpensesOpenChange}
            />
          </PermissionGuard>

          <PermissionGuard permissions={['income:view']} mode="all">
            <IncomesOverview
              isOpen={dashboardState.incomesOpen}
              onOpenChange={handleIncomesOpenChange}
            />
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
    </div>
  );
}

export default DashboardPage;
