import { Plus } from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Outlet,
  useLocation,
  useNavigate,
  useSearchParams,
} from 'react-router';

import { useApiGetAllAccounts, useApiGetAllExpenses } from '@/api/hooks';
import {
  EmptyStateMessage,
  ErrorSheet,
  LoadingOverlay,
} from '@/components/feedback';
import {
  AccountFilter,
  FilterResultsCount,
  SearchInput,
} from '@/components/filters';
import { Toolbar } from '@/components/layout';
import { Button } from '@/components/ui/button';
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip';
import { logger } from '@/concerns';
import { useErrorContext } from '@/contexts';
import type { Expense } from '@/data/expense';
import { useAccountFilter } from '@/hooks';
import { useIsMobile } from '@/hooks/use-mobile';
import { useIsShortViewport } from '@/hooks/use-short-viewport';

import { WithPermission } from '../auth/components';
import { ExpenseCardGrid, ExpensesHeader, ExpensesTable } from './components';
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
  const isMobile = useIsMobile();
  const isShortViewport = useIsShortViewport();

  // On short viewports (e.g. phone landscape) the fixed header + toolbar can
  // crowd out the table region entirely, so scroll the whole content area below
  // the header instead of only the table/card region.
  const contentAreaClass = isShortViewport
    ? 'flex-1 min-h-0 flex flex-col gap-4 p-6 overflow-y-auto'
    : 'flex-1 min-h-0 flex flex-col gap-4 p-6';
  const listRegionClass = isShortViewport
    ? 'relative'
    : 'flex-1 min-h-0 flex flex-col relative';

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
    () => (expensesResult?.success ? expensesResult.value : []),
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

  /**
   * Account filter state model:
   * - Render-time filter state comes only from the URL `accountId` query parameter.
   * - This page writes storage but does not read storage for rendering.
   * - The sidebar reads storage to build feature links with the last selected account.
   *
   * Storage is written from explicit interactions and URL sync paths:
   * - Badge navigation writes storage before route change.
   * - Filter changes write storage via `updateSelectedAccount`.
   * - URL-driven account changes are promoted to storage by the consolidated effect.
   */
  const location = useLocation();
  const urlAccountId = searchParams.get('accountId');

  // RULE 1: URL is the sole render-time source of truth. No storage fallback.
  // If URL has ?accountId, use it. Otherwise, null (explicitly unfiltered, not a fallback).
  const selectedAccountId = urlAccountId || null;

  // Derive isSubRoute from React Router location, not fragile window.location.pathname.includes()
  // isSubRoute = true when in an edit or create flow (sub-routes of the expenses list)
  const isSubRoute = useMemo(() => {
    const pathname = location.pathname;
    // Match: /expenses/edit/<id>, /expenses/edit, /expenses/create
    return (
      pathname.includes('/edit/') ||
      pathname.includes('/edit') ||
      pathname.includes('/create')
    );
  }, [location.pathname]);

  // Memoize updateSelectedAccount to satisfy dependency array requirements.
  // This function updates storage and optionally the URL when a filter is explicitly selected.
  // When isSubRoute=true (inside edit/create flow), skips URL update to preserve list filter state.
  const updateSelectedAccount = useCallback(
    (accountId: string | null) => {
      setExpenseData({ selectedAccountId: accountId });

      if (isSubRoute) {
        return;
      }

      if (accountId) {
        const newSearchParams = new URLSearchParams();
        newSearchParams.set('accountId', accountId);

        setSearchParams(newSearchParams, { replace: true });

        return;
      }

      setSearchParams(new URLSearchParams(), { replace: true });
    },
    [isSubRoute, setExpenseData, setSearchParams],
  );

  // Base-route mount behavior:
  // when there is no `accountId` in the URL and we are on the list route,
  // clear stored account selection so subsequent sidebar links stay unfiltered.
  useEffect(() => {
    // Only clear storage when:
    // 1. No URL account param (base route)
    // 2. Not in a sub-route (edit/create flows don't affect filter)
    // 3. Accounts query is resolved before writing storage
    if (urlAccountId !== null || isSubRoute) {
      return;
    }

    if (!accountsResult?.success) {
      return;
    }

    // Base route with no URL param: write null to storage to clear filter
    setExpenseData({ selectedAccountId: null });
  }, [urlAccountId, isSubRoute, accountsResult?.success, setExpenseData]);

  // Consolidated URL -> storage sync:
  // - clears invalid account IDs from URL/storage
  // - promotes valid URL account IDs into storage so sidebar links stay aligned
  // - skips edit/create sub-routes to avoid mutating list URL state mid-flow
  useEffect(() => {
    if (
      !urlAccountId ||
      isSubRoute ||
      accountsLoading ||
      accountsFetching ||
      !accountsResult?.success
    ) {
      return;
    }

    if (urlAccountId === 'not-assigned') {
      // 'not-assigned' is a valid special token representing unfiltered state.
      // It should be promoted to storage just like any other valid account ID.
      setExpenseData({ selectedAccountId: urlAccountId });
      return;
    }

    const hasMatchingAccount = accounts.some(
      account => account.rowId.toString() === urlAccountId,
    );

    if (!hasMatchingAccount) {
      // Invalid account (deleted or never existed): clear URL and storage.
      updateSelectedAccount(null);
      return;
    }

    // Valid account in URL and differs from storage: promote to storage.
    const storedData = getExpenseData();
    const storedSelectedAccountId = storedData?.selectedAccountId || null;

    if (urlAccountId !== storedSelectedAccountId) {
      setExpenseData({ selectedAccountId: urlAccountId });
    }
  }, [
    urlAccountId,
    isSubRoute,
    accountsLoading,
    accountsFetching,
    accountsResult?.success,
    accounts,
    getExpenseData,
    setExpenseData,
    updateSelectedAccount,
  ]);

  // Use the shared account filtering hook with controlled state
  const {
    accountsInItems,
    filteredItems: filteredExpenses,
    setSelectedAccountId: handleAccountChange,
  } = useAccountFilter<Expense>({
    accounts,
    items: expenses,
    selectedAccountId,
    onAccountChange: updateSelectedAccount,
  });

  // Validate the selected account ID for display in header
  const validatedSelectedAccountId = useMemo(() => {
    if (!selectedAccountId) {
      return null;
    }

    // Check if it's a valid account that has items
    const isValidAccount = accountsInItems.some(
      account => account.rowId.toString() === selectedAccountId,
    );

    return isValidAccount ? selectedAccountId : null;
  }, [selectedAccountId, accountsInItems]);

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
  const createPath = location.search ? `create${location.search}` : 'create';

  const handleSearchTermChange = (term: string) => {
    const trimmedTerm = term.trim();
    setSearchTerm(trimmedTerm);
    setExpenseData({ filterDescription: trimmedTerm });
  };

  const handleClearFilters = () => {
    setSearchTerm('');
    setExpenseData({ filterDescription: '', selectedAccountId: null });
    handleAccountChange(null);
  };

  const hasNoAccounts = !isLoading && accounts.length === 0;
  const hasNoExpenses =
    !isLoading &&
    accounts.length > 0 &&
    descriptionFilteredExpenses.length === 0;
  const hasActiveFilters =
    searchTerm.length > 0 || validatedSelectedAccountId !== null;

  return (
    <div className="flex flex-col h-screen overflow-hidden bg-gradient-to-br from-background to-muted/20">
      <ExpensesHeader />
      {/* Main content area: flex-1 min-h-0 for proper flexbox scrolling */}
      <div className={contentAreaClass}>
        <Toolbar>
          <div className="w-full space-y-3">
            <div className="flex flex-col sm:flex-row gap-3">
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
            </div>
            <div className="flex items-center justify-between gap-3">
              <FilterResultsCount
                filteredCount={descriptionFilteredExpenses.length}
                totalCount={expenses.length}
                isFilterActive={searchTerm.length > 0}
              />
              <WithPermission permissions={['expense:manage']} mode="all">
                {accounts.length === 0 ? (
                  <Tooltip>
                    <TooltipTrigger asChild>
                      <span className="inline-flex" tabIndex={0}>
                        <Button
                          onClick={() => navigate(createPath)}
                          aria-label="Add a new expense"
                          size="sm"
                          variant="default"
                          className="gap-1.5 font-medium shadow-sm bg-primary active:scale-95 transition-transform"
                          disabled
                        >
                          <Plus className="h-4 w-4" />
                          <span className="hidden sm:inline">Add Expense</span>
                        </Button>
                      </span>
                    </TooltipTrigger>
                    <TooltipContent>
                      Create at least one account first.
                    </TooltipContent>
                  </Tooltip>
                ) : (
                  <Button
                    onClick={() => navigate(createPath)}
                    aria-label="Add a new expense"
                    size="sm"
                    variant="default"
                    className="gap-1.5 font-medium shadow-sm bg-primary active:scale-95 transition-transform"
                  >
                    <Plus className="h-4 w-4" />
                    <span className="hidden sm:inline">Add Expense</span>
                  </Button>
                )}
              </WithPermission>
            </div>
          </div>
        </Toolbar>
        {/* Table container: flex-1 min-h-0 for proper flexbox, no overflow here */}
        <div className={listRegionClass}>
          {isLoading && <LoadingOverlay />}
          {hasNoAccounts ? (
            <EmptyStateMessage
              title="Create an account to get started"
              description="Every expense entry must be assigned to an account."
              primaryActionLabel="Create Account"
              onPrimaryAction={() => navigate('/accounts/create')}
            />
          ) : hasNoExpenses ? (
            <EmptyStateMessage
              title={
                hasActiveFilters ? 'No matching expenses' : 'No expenses yet'
              }
              description={
                hasActiveFilters
                  ? 'Try adjusting your search to find matching expenses.'
                  : 'Add your first expense to start tracking upcoming obligations.'
              }
              primaryActionLabel={
                hasActiveFilters ? 'Clear filters' : 'Add first expense'
              }
              onPrimaryAction={
                hasActiveFilters
                  ? handleClearFilters
                  : () => {
                      navigate(createPath);
                    }
              }
            />
          ) : isMobile ? (
            <ExpenseCardGrid expenses={descriptionFilteredExpenses} />
          ) : (
            <ExpensesTable filteredExpenses={descriptionFilteredExpenses} />
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
      <Outlet />
    </div>
  );
}

export default ExpensesPage;
