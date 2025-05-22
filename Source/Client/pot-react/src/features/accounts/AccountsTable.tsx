import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router';

import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import { ErrorSheet } from '@/components/feedback/sheet/ErrorSheet';
import { DataTable } from '@/components/table/DataTable';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { DisplayError } from '@/lib/errors/displayError';

import { columns } from './columns';
import {
  AccountsSummary,
  useAccountsSummaryStore,
} from './hooks/useAccountsSummary';

function AccountsTable() {
  const { id: editingId } = useParams<{ id: string }>();
  const { data: result, isLoading } = useApiGetAllAccounts();
  const [error, setError] = useState<DisplayError | null>(null);
  const navigate = useNavigate();

  const setSummary = useAccountsSummaryStore(
    (state: AccountsSummary) => state.setSummary,
  );

  // Memoize 'accounts' so its array reference only changes when `result` changes.
  // Without this, each render would create a new array (even with identical data), causing the summary effect to rerun every time.
  const accounts = useMemo(() => {
    return result?.success ? result.value : [];
  }, [result]);

  // Recalculates and pushes summary; 'setSummary' is in deps to satisfy exhaustive-deps and use the latest setter
  useEffect(() => {
    const totalBalance = accounts.reduce((sum, acct) => sum + acct.balance, 0);

    const totalReserved = accounts.reduce(
      (sum, acct) => sum + acct.reserved,
      0,
    );

    const totalAllocated = accounts.reduce(
      (sum, acct) => sum + acct.allocated,
      0,
    );

    const totalDailyAccrual = accounts.reduce(
      (sum, acct) => sum + acct.dailyAccrual,
      0,
    );

    setSummary(totalBalance, totalReserved, totalAllocated, totalDailyAccrual);
  }, [accounts, setSummary]);

  useEffect(() => {
    // Transient error handling - resetting error state when success, such as after a network loss
    if (result) {
      setError(
        result.success
          ? null
          : {
              title: result.error.code,
              description: result.error.description,
            },
      );
    }
  }, [result]);

  return (
    <>
      <Card>
        <CardHeader className="flex flex-row items-center">
          <div>
            <CardTitle>Bank Accounts</CardTitle>
            <CardDescription>Manage your account details.</CardDescription>
          </div>
          {/*
           * ml-auto pushes the element as far right as it can go in a flex container
           * flex makes it a flex container
           * items-center vertically centers its children
           * space-x-4 adds a 1rem horizontal gap between each child
           */}
          <div className="ml-auto flex items-center space-x-4">
            <Button
              onClick={() => navigate('create')}
              aria-label="Add a new account"
            >
              Add Account
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={accounts}
            highlightRowFilter={row =>
              row.original.rowId.toString() === editingId
            }
          />
        </CardContent>
      </Card>
      <LoadingMessage isLoading={isLoading} />
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
    </>
  );
}

export { AccountsTable };
