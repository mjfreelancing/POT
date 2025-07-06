import { Plus } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Outlet, useSearchParams } from 'react-router';
import { useNavigate } from 'react-router';

import { useApiGetAllAccounts, useApiGetAllExpenses } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import AccountFilter from '@/components/filters/AccountFilter';
import Toolbar from '@/components/toolbar/Toolbar';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Expense } from '@/data/expense';
import { useAccountFilter } from '@/hooks';
import { DisplayError } from '@/lib';

import { ExpensesHeader, ExpensesTable } from './components';

function ExpensesPage() {
  console.info('Rendering ExpensesPage');

  const [searchParams, setSearchParams] = useSearchParams();
  const [error, setError] = useState<DisplayError | null>(null);
  const [searchTerm, setSearchTerm] = useState<string>('');

  // Get data
  const { data: expensesResult, isLoading: expensesLoading } =
    useApiGetAllExpenses();

  const { data: accountsResult, isLoading: accountsLoading } =
    useApiGetAllAccounts();

  // Memoize data arrays to prevent unnecessary re-renders
  const expenses = useMemo(
    () => (expensesResult?.success ? expensesResult.value.results : []),
    [expensesResult],
  );
  const accounts = useMemo(
    () => (accountsResult?.success ? accountsResult.value : []),
    [accountsResult],
  );

  const isLoading = expensesLoading || accountsLoading;

  // Get account filter from URL
  const urlAccountId = searchParams.get('accountId');

  // Use the shared account filtering hook to manage filtering state
  const {
    accountsInItems,
    selectedAccountId,
    setSelectedAccountId,
    filteredItems: filteredExpenses,
  } = useAccountFilter<Expense>({
    accounts,
    items: expenses,
  });

  // Validate and sync filter state with URL, handling all edge cases
  useEffect(() => {
    if (accounts.length > 0 && expenses.length > 0) {
      if (urlAccountId) {
        // Check if the account from URL exists
        const accountExists = accounts.some(
          account => account.rowId.toString() === urlAccountId,
        );

        if (accountExists) {
          // Check if the account has items after filtering
          const accountHasItems = expenses.some(expense => {
            if (urlAccountId === 'not-assigned') {
              return !expense.account?.rowId;
            }
            return expense.account?.rowId?.toString() === urlAccountId;
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
    expenses,
    selectedAccountId,
    setSelectedAccountId,
    setSearchParams,
  ]);

  // Monitor filtered results and clear URL if last item was deleted
  useEffect(() => {
    if (
      urlAccountId &&
      selectedAccountId === urlAccountId &&
      filteredExpenses.length === 0 &&
      expenses.length > 0
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
    filteredExpenses.length,
    expenses.length,
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
  }, [expensesResult]);

  const navigate = useNavigate();

  return (
    <div className="flex flex-col h-screen bg-gradient-to-br from-background to-muted/20">
      <ExpensesHeader />
      {/* Main content area: flex-1 min-h-0 for proper flexbox scrolling */}
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
            <Input
              type="text"
              placeholder="Search by description..."
              value={searchTerm}
              onChange={e => setSearchTerm(e.target.value)}
              className="w-80"
              aria-label="Search expenses by description"
              name="expense-search"
            />
          </div>
          <Button
            onClick={() => navigate('create')}
            aria-label="Add a new expense"
            className="gap-2 min-w-[132px]"
            disabled={accounts.length === 0}
          >
            <Plus className="h-4 w-4" />
            Add Expense
          </Button>
        </Toolbar>
        {/* Table container: flex-1 min-h-0 for proper flexbox, no overflow here */}
        <div className="flex-1 min-h-0 flex flex-col">
          <ExpensesTable filteredExpenses={descriptionFilteredExpenses} />
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
      <Outlet />
    </div>
  );
}

export default ExpensesPage;
