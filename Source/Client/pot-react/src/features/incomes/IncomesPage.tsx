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
import { DisplayError, logger } from '@/lib';

import { IncomesHeader, IncomesTable } from './components';
import useIncomeStorage from './hooks/useIncomeStorage';
import { WithPermission } from '../auth/components';

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

  // Get account filter from storage and/or URL
  const urlAccountId = searchParams.get('accountId');
  const storedData = getIncomeData();
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
    filteredItems: filteredIncomes,
    setSelectedAccountId: handleAccountChange,
  } = useAccountFilter<Income>({
    accounts,
    items: incomes,
    selectedAccountId: urlAccountId || storedData?.selectedAccountId || null,
    onAccountChange: accountId => {
      if (accountId) {
        // Always update storage
        setIncomeData({ selectedAccountId: accountId });

        // Only update URL if not editing
        if (!isEditing) {
          const newSearchParams = new URLSearchParams();
          newSearchParams.set('accountId', accountId);
          setSearchParams(newSearchParams);
        }
      } else {
        // Always update storage
        setIncomeData({ selectedAccountId: null });

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
          <WithPermission permission="income:manage">
            <Button
              onClick={() => navigate('create')}
              aria-label="Add a new income"
              className="gap-2 min-w-[132px]"
              disabled={accounts.length === 0}
            >
              <Plus className="h-4 w-4" />
              Add Income
            </Button>
          </WithPermission>
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
