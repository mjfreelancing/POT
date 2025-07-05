import { useEffect, useMemo, useState } from 'react';
import { Outlet } from 'react-router';

import { useApiGetAllAccounts } from '@/api/hooks';
import { ErrorSheet, LoadingMessage } from '@/components/feedback';
import { DisplayError } from '@/lib';

import { AccountsHeader, AccountsTable } from './components';

function AccountsPage() {
  console.info('Rendering AccountsPage');

  const [error, setError] = useState<DisplayError | null>(null);

  const { data: accountsResult, isLoading } = useApiGetAllAccounts();

  // Memoize accounts array to prevent unnecessary re-renders
  const accounts = useMemo(
    () => (accountsResult?.success ? accountsResult.value : []),
    [accountsResult],
  );

  // Handle errors
  useEffect(() => {
    if (accountsResult) {
      setError(
        accountsResult.success
          ? null
          : {
              title: accountsResult.error.code,
              description: accountsResult.error.description,
            },
      );
    }
  }, [accountsResult]);

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20">
      <AccountsHeader />
      <div className="flex-1 p-6 overflow-hidden">
        <AccountsTable accounts={accounts} />
      </div>
      <LoadingMessage isLoading={isLoading} />
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
      {/* The Outlet is used to render nested routes, such as when creating/editing accounts */}
      <Outlet />
    </div>
  );
}

export default AccountsPage;
