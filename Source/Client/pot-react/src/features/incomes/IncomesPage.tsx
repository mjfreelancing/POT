import { Plus } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Outlet, useSearchParams } from 'react-router';
import { useNavigate } from 'react-router';

import { useApiGetAllAccounts, useApiGetAllIncomes } from '@/api/hooks';
import useIncomeStorage from './hooks/useIncomeStorage';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { AccountFilter, SearchInput } from '@/components/filters';
import Toolbar from '@/components/toolbar/Toolbar';
import { Button } from '@/components/ui/button';
import type { Income } from '@/data/income';
import { useAccountFilter } from '@/hooks';
import { DisplayError, logger } from '@/lib';

import { IncomesHeader, IncomesTable } from './components';

function IncomesPage() {
  logger.info('IncomesPage', 'Rendering');

  const [searchParams, setSearchParams] = useSearchParams();
  const [error, setError] = useState<DisplayError | null>(null);
  const [searchTerm, setSearchTerm] = useState<string>('');

  // Setup local storage for persistent filters
  const { getIncomeData, setIncomeData } = useIncomeStorage(error => {
    setError({
      title: 'Storage Error',
      description: error.description,
    });
  });

  // Get data
  const { data: incomesResult, isLoading: incomesLoading } =
    useApiGetAllIncomes();
  const { data: accountsResult, isLoading: accountsLoading } =
    useApiGetAllAccounts();

  // Memoize data arrays to prevent unnecessary re-renders
  const incomes = useMemo(
    () => (incomesResult?.success ? incomesResult.value : []),
    [incomesResult],
  );
  const accounts = useMemo(
    () => (accountsResult?.success ? accountsResult.value : []),
    [accountsResult],
  );

  const isLoading = incomesLoading || accountsLoading;

  // Get account filter from URL
  const urlAccountId = searchParams.get('accountId');

  // Use the shared account filtering hook to manage filtering state
  const {
    accountsInItems,
    selectedAccountId,
    setSelectedAccountId,
    filteredItems: filteredIncomes,
  } = useAccountFilter<Income>({
    accounts,
    items: incomes,
  });

  // One-time initial hydration from storage if no URL parameter
  useEffect(() => {
    // Skip if no data yet or if URL parameter exists (URL takes precedence)
    if (accounts.length === 0 || incomes.length === 0 || urlAccountId) {
      return;
    }

    const storedData = getIncomeData();
    const storedAccountId = storedData?.selectedAccountId;

    if (storedAccountId) {
      // Check if account exists and has items
      const accountExists = accounts.some(
        account => account.rowId.toString() === storedAccountId,
      );

      const hasItems =
        !storedAccountId ||
        incomes.some(
          income => income.account?.rowId?.toString() === storedAccountId,
        );

      if (accountExists && hasItems) {
        setSelectedAccountId(storedAccountId);

        const newSearchParams = new URLSearchParams(searchParams);
        newSearchParams.set('accountId', storedAccountId);

        setSearchParams(newSearchParams);
      }
    }
  }, [accounts.length, incomes.length]); // Only run on initial data load

  // Keep selectedAccountId in sync with URL
  useEffect(() => {
    if (urlAccountId) {
      // URL has an account - sync state to match
      if (selectedAccountId !== urlAccountId) {
        setSelectedAccountId(urlAccountId);
        setIncomeData({ selectedAccountId: urlAccountId });
      }
    } else {
      // URL has no account - ensure state is cleared
      if (selectedAccountId !== null) {
        setSelectedAccountId(null);
        setIncomeData({ selectedAccountId: null });
      }
    }
  }, [urlAccountId]);

  // Handle account filter changes from header
  const handleAccountChange = (accountId: string | null) => {
    if (!accountId) {
      setSelectedAccountId(null);
      setSearchParams(new URLSearchParams());
      setIncomeData({ selectedAccountId: null });
    } else {
      const newSearchParams = new URLSearchParams();
      newSearchParams.set('accountId', accountId);

      setSelectedAccountId(accountId);
      setSearchParams(newSearchParams);
      setIncomeData({ selectedAccountId: accountId });
    }
  };

  // Validate the selected account ID for display in header
  const validatedSelectedAccountId = useMemo(() => {
    if (!urlAccountId) {
      return null;
    }

    // Check if it's a valid account that has items
    const isValidAccount = accountsInItems.some(
      account => account.rowId.toString() === urlAccountId,
    );

    return isValidAccount ? urlAccountId : null;
  }, [urlAccountId, accountsInItems]);

  // Filter incomes by description (case-insensitive)
  const descriptionFilteredIncomes = useMemo(() => {
    if (!searchTerm.trim()) {
      return filteredIncomes;
    }

    return filteredIncomes.filter(income =>
      income.description
        ?.toLowerCase()
        .includes(searchTerm.trim().toLowerCase()),
    );
  }, [filteredIncomes, searchTerm]);

  const navigate = useNavigate();

  // Handle errors
  useEffect(() => {
    if (incomesResult) {
      setError(
        incomesResult.success
          ? null
          : {
              title: incomesResult.error.code,
              description: incomesResult.error.description,
            },
      );
    }
  }, [incomesResult]);

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20">
      <IncomesHeader />
      <div className="flex-1 min-h-0 flex flex-col p-6 gap-4">
        <Toolbar>
          <div className="flex items-center gap-4">
            {accountsInItems.length > 1 && (
              <AccountFilter
                accounts={accountsInItems}
                selectedAccountId={validatedSelectedAccountId}
                onAccountChange={handleAccountChange}
              />
            )}
            <SearchInput
              value={searchTerm}
              onChange={setSearchTerm}
              placeholder="Search by description..."
              ariaLabel="Search incomes by description"
              name="income-search"
            />
          </div>
          <Button
            onClick={() => navigate('create')}
            aria-label="Add a new income"
            className="gap-2 min-w-[132px]"
            disabled={accounts.length === 0}
          >
            <Plus className="h-4 w-4" />
            Add Income
          </Button>
        </Toolbar>
        <div className="flex-1 min-h-0 flex flex-col">
          <IncomesTable filteredIncomes={descriptionFilteredIncomes} />
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
      {/* The Outlet is used to render nested routes, such as when creating/editing incomes */}
      <Outlet />
    </div>
  );
}

export default IncomesPage;
