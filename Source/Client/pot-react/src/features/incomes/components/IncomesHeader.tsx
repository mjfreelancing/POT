import { Plus } from 'lucide-react';
import { useEffect, useMemo } from 'react';
import { useNavigate, useSearchParams } from 'react-router';

import { useApiGetAllAccounts, useApiGetAllIncomes } from '@/api/hooks';
import AccountFilter from '@/components/filters/AccountFilter';
import { AppSidebarTrigger } from '@/components/nav';
import { Button } from '@/components/ui/button';
import { useAccountFilter } from '@/hooks';

function IncomesHeader() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  // Get data for account filtering
  const { data: incomesResult } = useApiGetAllIncomes();
  const { data: accountsResult } = useApiGetAllAccounts();

  const incomes = incomesResult?.success ? incomesResult.value.results : [];
  const accounts = useMemo(
    () => (accountsResult?.success ? accountsResult.value : []),
    [accountsResult],
  );

  // Get initial account filter from URL
  const initialAccountId = searchParams.get('accountId');

  // Use the shared account filtering hook
  const { accountsInItems, selectedAccountId, setSelectedAccountId } =
    useAccountFilter({
      accounts,
      items: incomes,
    });

  // Set initial account filter from URL when data is loaded
  useEffect(() => {
    if (initialAccountId && accounts.length > 0 && !selectedAccountId) {
      const accountExists = accounts.some(
        account => account.rowId.toString() === initialAccountId,
      );
      if (accountExists) {
        setSelectedAccountId(initialAccountId);
      }
    }
  }, [initialAccountId, accounts, selectedAccountId, setSelectedAccountId]);

  // Sync the filter with URL parameters
  const handleAccountChange = (accountId: string | null) => {
    setSelectedAccountId(accountId);
    if (accountId && accountId !== 'not-assigned') {
      setSearchParams({ accountId });
    } else {
      setSearchParams({});
    }
  };

  return (
    <div className="page-header">
      <div className="flex items-center justify-between">
        <div className="flex items-center">
          <AppSidebarTrigger />
          <div className="flex items-center gap-3">
            <div>
              <h1 className="page-title">Income Management</h1>
              <p className="page-subtitle">
                Track and manage your income sources
              </p>
            </div>
          </div>
        </div>
        <div className="flex items-center space-x-4">
          {accountsInItems.length > 1 && (
            <AccountFilter
              accounts={accountsInItems}
              selectedAccountId={selectedAccountId}
              onAccountChange={handleAccountChange}
            />
          )}
          <Button
            onClick={() => navigate('create')}
            aria-label="Add a new income source"
            className="gap-2"
          >
            <Plus className="h-4 w-4" />
            Add Income
          </Button>
        </div>
      </div>
    </div>
  );
}

export default IncomesHeader;
