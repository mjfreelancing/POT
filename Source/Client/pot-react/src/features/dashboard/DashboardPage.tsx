import { useEffect, useState } from 'react';

import { useApiGetAllAccounts } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { Toaster } from '@/components/ui/sonner';
import { DisplayError } from '@/lib';

import { AccountsOverview, DashboardHeader, QuickActions } from './components';

function DashboardPage() {
  const [error, setError] = useState<DisplayError | null>(null);
  const { data: result, isLoading } = useApiGetAllAccounts();

  useEffect(() => {
    if (result) {
      if (result.success) {
        setError(null);
      } else {
        setError({
          title: result.error.code,
          description: result.error.description,
        });
      }
    }
  }, [result]);

  const accounts = result?.success ? result.value : [];

  return (
    <div className="flex flex-col flex-1 min-h-screen bg-gradient-to-br from-background to-muted/20">
      <Toaster position="top-center" />
      <DashboardHeader />

      <LoadingMessage isLoading={isLoading} />

      <div className="flex-1 p-6 space-y-6">
        <QuickActions />
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
