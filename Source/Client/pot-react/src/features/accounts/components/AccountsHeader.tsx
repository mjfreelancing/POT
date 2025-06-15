import { Calendar, DollarSign, Plus, Target, Wallet } from 'lucide-react';
import { useNavigate } from 'react-router';

import { ActionCard } from '@/components/cards';
import { AppSidebarTrigger } from '@/components/nav';
import { Button } from '@/components/ui/button';
import { formatMoneyValue } from '@/lib';

import accountsSummaryStore, {
  AccountsSummary,
} from '../stores/useAccountsSummary';

function AccountsHeader() {
  const navigate = useNavigate();

  const totalBalance = accountsSummaryStore(
    (state: AccountsSummary) => state.totalBalance,
  );

  const totalReserved = accountsSummaryStore(
    (state: AccountsSummary) => state.totalReserved,
  );

  const totalAllocated = accountsSummaryStore(
    (state: AccountsSummary) => state.totalAllocated,
  );

  const totalDailyAccrual = accountsSummaryStore(
    (state: AccountsSummary) => state.totalDailyAccrual,
  );

  return (
    <div className="page-header">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center">
          <AppSidebarTrigger />
          <div>
            <h1 className="page-title">Account Management</h1>
            <p className="page-subtitle">
              Monitor and manage your financial accounts
            </p>
          </div>
        </div>
        <Button
          onClick={() => navigate('create')}
          aria-label="Add a new account"
          className="gap-2 min-w-[132px]"
        >
          <Plus className="h-4 w-4" />
          Add Account
        </Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <ActionCard
          title="Total Balance"
          icon={<Wallet className="h-6 w-6" />}
          className="border-2"
        >
          <div className="text-2xl font-bold text-success">
            {formatMoneyValue(totalBalance)}
          </div>
        </ActionCard>
        <ActionCard
          title="Reserved Funds"
          icon={<DollarSign className="h-6 w-6" />}
          className="border-2"
        >
          <div className="text-2xl font-bold">
            {formatMoneyValue(totalReserved)}
          </div>
        </ActionCard>
        <ActionCard
          title="Allocated Amount"
          icon={<Target className="h-6 w-6" />}
          className="border-2"
        >
          <div className="text-2xl font-bold">
            {formatMoneyValue(totalAllocated)}
          </div>
        </ActionCard>
        <ActionCard
          title="Daily Accrual"
          icon={<Calendar className="h-6 w-6" />}
          className="border-2"
        >
          <div
            className={`text-2xl font-bold ${totalDailyAccrual > 0 ? 'text-success' : 'text-destructive'}`}
          >
            {formatMoneyValue(totalDailyAccrual)}
          </div>
        </ActionCard>
      </div>
    </div>
  );
}

export default AccountsHeader;
