import { Plus } from 'lucide-react';
import { useMemo, useState } from 'react';
import { useEffect } from 'react';
import { Outlet, useSearchParams } from 'react-router';
import { useNavigate } from 'react-router';

import { useApiGetAllAccounts, useApiGetAllExpenses } from '@/api/hooks';
import { ErrorSheet, LoadingOverlay } from '@/components/feedback';
import {
  AccountFilter,
  FilterResultsCount,
  SearchInput,
} from '@/components/filters';
import { Toolbar } from '@/components/layout';
import { Button } from '@/components/ui/button';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { Expense } from '@/data/expense';
import { useAccountFilter } from '@/hooks';

import { WithPermission } from '../auth/components';
import { ExpensesHeader, ExpensesTable } from './components';
import useExpenseStorage from './hooks/useExpenseStorage';

function ExpensesPage() {
  useEffect(() => {
    logger.info('ExpensesPage', 'Mounted');

    return () => {
      logger.info('ExpensesPage', 'Unmounted');
    };
  }, []);

  const [searchParams, setSearchParams] = useSearchParams();
  const { error, setError } = useErrorContext();
  const [searchTerm, setSearchTerm] = useState<string>('');

  // Setup local storage for persistent filters
  const { getExpenseData, setExpenseData } = useExpenseStorage(error => {
    setError({
      title: 'Storage Error',
      description: error.description,
    });
  });

  // Apply stored description filter on mount
  useEffect(() => {
    const storedFilter = getExpenseData().filterDescription;

    if (storedFilter) {
      setSearchTerm(storedFilter);
    }
  }, [getExpenseData]);

  const {
    data: expensesResult,
    isLoading: expensesLoading,
    isFetching: expensesFetching,
  } = useApiGetAllExpenses();

  const {
    data: accountsResult,
    isLoading: accountsLoading,
    isFetching: accountsFetching,
  } = useApiGetAllAccounts();

  // Memoize data arrays to prevent unnecessary re-renders
  const expenses = useMemo(
    () => (expensesResult?.success ? expensesResult.value.results : []),
    [expensesResult],
  );
  const accounts = useMemo(
    () => (accountsResult?.success ? accountsResult.value : []),
    [accountsResult],
  );

  const isLoading = useMemo(
    () =>
      expensesLoading ||
      accountsLoading ||
      expensesFetching ||
      accountsFetching,
    [expensesLoading, accountsLoading, expensesFetching, accountsFetching],
  );

  // Get account filter from storage and/or URL
  const urlAccountId = searchParams.get('accountId');
  const storedData = getExpenseData();
  const isEditing = window.location.pathname.includes('/edit/');

  // When returning from edit mode, restore URL from storage if needed
  useEffect(() => {
    if (!isEditing && !urlAccountId && storedData?.selectedAccountId) {
      const newSearchParams = new URLSearchParams();
      newSearchParams.set('accountId', storedData.selectedAccountId);

      setSearchParams(newSearchParams);
    }
  }, [isEditing, urlAccountId, storedData?.selectedAccountId, setSearchParams]);

  // Use the shared account filtering hook with controlled state
  const {
    accountsInItems,
    filteredItems: filteredExpenses,
    setSelectedAccountId: handleAccountChange,
  } = useAccountFilter<Expense>({
    accounts,
    items: expenses,
    selectedAccountId: urlAccountId || storedData?.selectedAccountId || null,
    onAccountChange: accountId => {
      if (accountId) {
        // Always update storage
        setExpenseData({ selectedAccountId: accountId });

        // Only update URL if not editing
        if (!isEditing) {
          const newSearchParams = new URLSearchParams();
          newSearchParams.set('accountId', accountId);

          setSearchParams(newSearchParams);
        }
      } else {
        // Always update storage
        setExpenseData({ selectedAccountId: null });

        // Only update URL if not editing
        if (!isEditing) {
          setSearchParams(new URLSearchParams());
        }
      }
    },
  });

  // Get initial account ID from URL or storage
  const initialAccountId = useMemo(
    () => urlAccountId || storedData?.selectedAccountId || null,
    [urlAccountId, storedData?.selectedAccountId],
  );

  // Validate the selected account ID for display in header
  const validatedSelectedAccountId = useMemo(() => {
    if (!initialAccountId) {
      return null;
    }

    // Check if it's a valid account that has items
    const isValidAccount = accountsInItems.some(
      account => account.rowId.toString() === initialAccountId,
    );

    return isValidAccount ? initialAccountId : null;
  }, [initialAccountId, accountsInItems]);

  // Filter expenses by description (case-insensitive)
  const descriptionFilteredExpenses = useMemo(() => {
    if (!searchTerm.trim()) {
      return filteredExpenses;
    }

    return filteredExpenses.filter(expense =>
      expense.description
        ?.toLowerCase()
        .includes(searchTerm.trim().toLowerCase()),
    );
  }, [filteredExpenses, searchTerm]);

  // Handle errors
  useEffect(() => {
    if (expensesResult) {
      setError(
        expensesResult.success
          ? null
          : {
              title: expensesResult.error.code,
              description: expensesResult.error.description,
            },
      );
    }
  }, [expensesResult, setError]);

  const navigate = useNavigate();

  const handleSearchTermChange = (term: string) => {
    setSearchTerm(term);
    setExpenseData({ filterDescription: term });
  };

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20">
      <ExpensesHeader />
      {/* Main content area: flex-1 min-h-0 for proper flexbox scrolling */}
      <div className="flex-1 min-h-0 flex flex-col p-6 gap-4">
        <Toolbar>
          <div className="flex flex-wrap items-center gap-3">
            <SearchInput
              value={searchTerm}
              onChange={handleSearchTermChange}
              placeholder="Search by description..."
              ariaLabel="Search expenses by description"
              name="expense-search"
            />
            {accountsInItems.length > 1 && (
              <AccountFilter
                accounts={accountsInItems}
                selectedAccountId={validatedSelectedAccountId}
                onAccountChange={handleAccountChange}
              />
            )}
            <FilterResultsCount
              filteredCount={descriptionFilteredExpenses.length}
              totalCount={expenses.length}
            />
          </div>
          <WithPermission permissions={['expense:manage']} mode="all">
            <Button
              onClick={() => navigate('create')}
              aria-label="Add a new expense"
              className="gap-2 min-w-[132px]"
              disabled={accounts.length === 0}
            >
              <Plus className="h-4 w-4" />
              Add Expense
            </Button>
          </WithPermission>
        </Toolbar>
        {/* Table container: flex-1 min-h-0 for proper flexbox, no overflow here */}
        <div className="flex-1 min-h-0 flex flex-col relative">
          {isLoading && <LoadingOverlay />}
          <ExpensesTable filteredExpenses={descriptionFilteredExpenses} />
        </div>
      </div>

      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
      <Outlet />
    </div>
  );
}

export default ExpensesPage;
