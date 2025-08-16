import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';

import {
  AccountsOverview,
  DashboardHeader,
  ExpensesOverview,
  QuickActions,
} from './components';
import { DashboardProvider, useDashboardContext } from './context';

function DashboardContent() {
  const { error, setError } = useDashboardContext();

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20 overflow-x-hidden">
      <DashboardHeader />

      <div className="flex-1 p-6 space-y-6">
        <QuickActions />
        <ExpensesOverview />
        <AccountsOverview />

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

function DashboardPage() {
  return (
    <DashboardProvider>
      <DashboardContent />
    </DashboardProvider>
  );
}

export default DashboardPage;
