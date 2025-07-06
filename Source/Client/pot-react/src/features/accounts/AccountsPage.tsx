import { Plus } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Outlet } from 'react-router';
import { useNavigate } from 'react-router';

import { useApiGetAllAccounts } from '@/api/hooks';
import { ErrorSheet, LoadingMessage } from '@/components/feedback';
import { SearchInput } from '@/components/filters';
import Toolbar from '@/components/toolbar/Toolbar';
import { Button } from '@/components/ui/button';
import { DisplayError } from '@/lib';

import { AccountsHeader, AccountsTable } from './components';

function AccountsPage() {
  console.info('Rendering AccountsPage');

  const [error, setError] = useState<DisplayError | null>(null);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const navigate = useNavigate();

  const { data: accountsResult, isLoading } = useApiGetAllAccounts();

  // Memoize accounts array to prevent unnecessary re-renders
  const accounts = useMemo(
    () => (accountsResult?.success ? accountsResult.value : []),
    [accountsResult],
  );

  // Filter accounts by description (case-insensitive)
  const descriptionFilteredAccounts = useMemo(() => {
    if (!searchTerm.trim()) {
      return accounts;
    }

    return accounts.filter(account =>
      account.description
        ?.toLowerCase()
        .includes(searchTerm.trim().toLowerCase()),
    );
  }, [accounts, searchTerm]);

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
      <div className="flex-1 min-h-0 flex flex-col p-6 gap-4">
        <Toolbar>
          <div className="flex items-center gap-4">
            <SearchInput
              value={searchTerm}
              onChange={setSearchTerm}
              placeholder="Search by description..."
              ariaLabel="Search accounts by description"
              name="account-search"
            />
          </div>
          <Button
            onClick={() => navigate('create')}
            aria-label="Add a new account"
            className="gap-2 min-w-[132px]"
          >
            <Plus className="h-4 w-4" />
            Add Account
          </Button>
        </Toolbar>
        <div className="flex-1 min-h-0 flex flex-col">
          <AccountsTable accounts={descriptionFilteredAccounts} />
        </div>
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
