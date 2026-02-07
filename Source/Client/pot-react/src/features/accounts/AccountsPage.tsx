import { Plus } from 'lucide-react';
import { useMemo, useState } from 'react';
import { useEffect } from 'react';
import { Outlet } from 'react-router';
import { useNavigate } from 'react-router';

import { useApiGetAllAccounts } from '@/api/hooks';
import { ErrorSheet, LoadingOverlay } from '@/components/feedback';
import { FilterResultsCount, SearchInput } from '@/components/filters';
import { Toolbar } from '@/components/layout';
import { Button } from '@/components/ui/button';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import { WithPermission } from '@/features/auth/components';
import { useIsMobile } from '@/hooks/use-mobile';

import { AccountCardGrid, AccountsHeader, AccountsTable } from './components';
import useAccountStorage from './hooks/useAccountStorage';

function AccountsPage() {
  useEffect(() => {
    logger.info('AccountsPage', 'Mounted');

    return () => {
      logger.info('AccountsPage', 'Unmounted');
    };
  }, []);

  const { error, setError } = useErrorContext();
  const [searchTerm, setSearchTerm] = useState<string>('');
  const navigate = useNavigate();
  const isMobile = useIsMobile();

  const {
    data: accountsData,
    isLoading: accountsLoading,
    isFetching: accountsFetching,
  } = useApiGetAllAccounts();

  const isLoading = useMemo(
    () => accountsLoading || accountsFetching,
    [accountsLoading, accountsFetching],
  );

  // Memoize accounts array to prevent unnecessary re-renders
  const accounts = useMemo(
    () => (accountsData?.success ? accountsData.value : []),
    [accountsData],
  );

  // Filter accounts by description (case-insensitive)
  const descriptionFilteredAccounts = useMemo(() => {
    if (!searchTerm) {
      return accounts;
    }

    return accounts.filter(account =>
      account.description?.toLowerCase().includes(searchTerm.toLowerCase()),
    );
  }, [accounts, searchTerm]);

  // Handle errors
  useEffect(() => {
    if (accountsData) {
      setError(
        accountsData.success
          ? null
          : {
              title: accountsData.error.code,
              description: accountsData.error.description,
            },
      );
    }
  }, [accountsData, setError]);

  const { getAccountData, setAccountData } = useAccountStorage(error => {
    setError({
      title: 'Storage Error',
      description: error.description,
    });
  });

  // Apply stored description filter on mount
  useEffect(() => {
    const storedFilter = getAccountData().filterDescription;

    if (storedFilter) {
      setSearchTerm(storedFilter);
    }
  }, [getAccountData]);

  const handleSearchTermChange = (term: string) => {
    const trimmedTerm = term.trim();
    setSearchTerm(trimmedTerm);
    setAccountData({ filterDescription: trimmedTerm });
  };

  return (
    <div className="flex flex-col h-screen overflow-hidden bg-gradient-to-br from-background to-muted/20">
      <AccountsHeader />
      <div className="flex-1 min-h-0 flex flex-col p-6 gap-4">
        <Toolbar>
          <div className="w-full space-y-3">
            <SearchInput
              value={searchTerm}
              onChange={handleSearchTermChange}
              placeholder="Search by description..."
              ariaLabel="Search accounts by description"
              name="account-search"
            />
            <div className="flex items-center justify-between gap-3">
              <FilterResultsCount
                filteredCount={descriptionFilteredAccounts.length}
                totalCount={accounts.length}
                isFilterActive={searchTerm.length > 0}
              />
              <WithPermission permissions={['account:manage']} mode="all">
                <Button
                  onClick={() => navigate('create')}
                  aria-label="Add a new account"
                  className="gap-2 min-w-[132px]"
                >
                  <Plus className="h-4 w-4" />
                  Add Account
                </Button>
              </WithPermission>
            </div>
          </div>
        </Toolbar>
        <div className="flex-1 min-h-0 flex flex-col relative">
          {isLoading && <LoadingOverlay />}
          {isMobile ? (
            <AccountCardGrid accounts={descriptionFilteredAccounts} />
          ) : (
            <AccountsTable accounts={descriptionFilteredAccounts} />
          )}
        </div>
      </div>

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
