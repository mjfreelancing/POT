import { Plus } from 'lucide-react';
import { useNavigate } from 'react-router';

import AccountFilter from '@/components/filters/AccountFilter';
import { AppSidebarTrigger } from '@/components/nav';
import { Button } from '@/components/ui/button';
import { Account } from '@/data';

type ExpensesHeaderProps = {
  accountsInItems: Account[];
  selectedAccountId: string | null;
  onAccountChange: (accountId: string | null) => void;
  totalAccountsCount: number;
};

function ExpensesHeader({
  accountsInItems,
  selectedAccountId,
  onAccountChange,
  totalAccountsCount,
}: ExpensesHeaderProps) {
  const navigate = useNavigate();

  return (
    <div className="page-header">
      <div className="flex items-center justify-between">
        <div className="flex items-center">
          <AppSidebarTrigger />
          <div className="flex items-center gap-3">
            <div>
              <h1 className="page-title">Expense Management</h1>
              <p className="page-subtitle">
                Track and categorize your expenses
              </p>
            </div>
          </div>
        </div>
        <div className="flex items-center space-x-4">
          {accountsInItems.length > 1 && (
            <AccountFilter
              accounts={accountsInItems}
              selectedAccountId={selectedAccountId}
              onAccountChange={onAccountChange}
            />
          )}
          <Button
            onClick={() => navigate('create')}
            aria-label="Add a new expense"
            className="gap-2 min-w-[132px]"
            disabled={totalAccountsCount === 0}
          >
            <Plus className="h-4 w-4" />
            Add Expense
          </Button>
        </div>
      </div>
    </div>
  );
}

export default ExpensesHeader;
export type { ExpensesHeaderProps };
