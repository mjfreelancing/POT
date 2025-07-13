import { useEffect, useState } from 'react';

import { useApiGetAllAccounts, useApiGetAllExpenses } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { Toaster } from '@/components/ui/sonner';
import { DisplayError } from '@/lib';

import {
  AccountsOverview,
  DashboardHeader,
  ExpensesOverview,
  QuickActions,
} from './components';

function DashboardPage() {
  const [error, setError] = useState<DisplayError | null>(null);
  const { data: accountsResult, isLoading: accountsIsLoading } =
    useApiGetAllAccounts();
  const { data: expensesResult, isLoading: expensesIsLoading } =
    useApiGetAllExpenses();
  const isLoading = accountsIsLoading || expensesIsLoading;

  useEffect(() => {
    if (accountsResult) {
      if (accountsResult.success) {
        setError(null);
      } else {
        setError({
          title: accountsResult.error.code,
          description: accountsResult.error.description,
        });
      }
    }
  }, [accountsResult]);

  const accounts = accountsResult?.success ? accountsResult.value : [];
  const expenses = expensesResult?.success ? expensesResult.value.results : [];

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20 overflow-x-hidden">
      <Toaster position="top-center" />
      <DashboardHeader />

      <LoadingMessage isLoading={isLoading} />

      <div className="flex-1 p-6 space-y-6">
        <QuickActions />
        <ExpensesOverview expenses={expenses} />
        <AccountsOverview accounts={accounts} />

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
