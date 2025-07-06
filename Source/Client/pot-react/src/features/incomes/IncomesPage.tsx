import { Plus } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Outlet, useSearchParams } from 'react-router';
import { useNavigate } from 'react-router';

import { useApiGetAllAccounts, useApiGetAllIncomes } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { AccountFilter, SearchInput } from '@/components/filters';
import Toolbar from '@/components/toolbar/Toolbar';
import { Button } from '@/components/ui/button';
import type { Income } from '@/data/income';
import { useAccountFilter } from '@/hooks';
import { DisplayError } from '@/lib';

import { IncomesHeader, IncomesTable } from './components';

function IncomesPage() {
  console.info('Rendering IncomesPage');

  const [searchParams, setSearchParams] = useSearchParams();
  const [error, setError] = useState<DisplayError | null>(null);
  const [searchTerm, setSearchTerm] = useState<string>('');

  // Get data
  const { data: incomesResult, isLoading: incomesLoading } =
    useApiGetAllIncomes();
  const { data: accountsResult, isLoading: accountsLoading } =
    useApiGetAllAccounts();

  // Memoize data arrays to prevent unnecessary re-renders
  const incomes = useMemo(
    () => (incomesResult?.success ? incomesResult.value.results : []),
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

  // Validate and sync filter state with URL, handling all edge cases
  useEffect(() => {
    if (accounts.length > 0 && incomes.length > 0) {
      if (urlAccountId) {
        // Check if the account from URL exists
        const accountExists = accounts.some(
          account => account.rowId.toString() === urlAccountId,
        );

        if (accountExists) {
          // Check if the account has items after filtering
          const accountHasItems = incomes.some(income => {
            if (urlAccountId === 'not-assigned') {
              return !income.account?.rowId;
            }
            return income.account?.rowId?.toString() === urlAccountId;
          });

          if (accountHasItems) {
            // Valid account with items - set the filter
            if (selectedAccountId !== urlAccountId) {
              setSelectedAccountId(urlAccountId);
            }
          } else {
            // Account exists but has no items - clear the URL filter
            console.log('Clearing account filter - no items:', urlAccountId);
            setSelectedAccountId(null);
            setSearchParams(new URLSearchParams());
          }
        } else {
          // Invalid account - clear the URL filter
          console.log('Clearing invalid account filter:', urlAccountId);
          setSelectedAccountId(null);
          setSearchParams(new URLSearchParams());
        }
      } else if (selectedAccountId) {
        // No URL filter but component has selection - clear it
        setSelectedAccountId(null);
      }
    }
  }, [
    urlAccountId,
    accounts,
    incomes,
    selectedAccountId,
    setSelectedAccountId,
    setSearchParams,
  ]);

  // Monitor filtered results and clear URL if last item was deleted
  useEffect(() => {
    if (
      urlAccountId &&
      selectedAccountId === urlAccountId &&
      filteredIncomes.length === 0 &&
      incomes.length > 0
    ) {
      // We have a filter active, but no filtered results while there are total items
      // This means the last item for this account was deleted
      console.log('Last filtered item deleted, clearing filter');
      setSelectedAccountId(null);
      setSearchParams(new URLSearchParams());
    }
  }, [
    urlAccountId,
    selectedAccountId,
    filteredIncomes.length,
    incomes.length,
    setSelectedAccountId,
    setSearchParams,
  ]);

  // Handle account filter changes from header
  const handleAccountChange = (accountId: string | null) => {
    setSelectedAccountId(accountId);
    const newSearchParams = new URLSearchParams(searchParams);
    if (accountId && accountId !== 'not-assigned') {
      newSearchParams.set('accountId', accountId);
    } else {
      newSearchParams.delete('accountId');
    }
    setSearchParams(newSearchParams);
  };

  // Validate the selected account ID for display in header
  const validatedSelectedAccountId = useMemo(() => {
    if (!urlAccountId) return null;

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
