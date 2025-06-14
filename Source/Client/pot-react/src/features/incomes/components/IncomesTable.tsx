import { ColumnDef } from '@tanstack/react-table';
import { useEffect, useMemo, useState } from 'react';
import { useParams, useSearchParams } from 'react-router';

import { useApiGetAllAccounts, useApiGetAllIncomes } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import {
  createDateColumn,
  createFrequencyColumn,
  createMoneyValueColumn,
  DataTable,
  DataTableColumnHeader,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import { Income } from '@/data';
import { useAccountFilter } from '@/hooks';
import { DisplayError } from '@/lib';

import IncomeActions from './IncomeActions';

const columns: ColumnDef<Income>[] = [
  {
    accessorKey: 'description',
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Description" />
    ),
    enableSorting: true,
    sortingFn: 'text',
  },
  {
    id: 'accountDescription',
    header: 'Account',
    cell: ({ row }) => (
      <div
        className={
          !row.original.account?.description ? 'text-muted-foreground' : ''
        }
      >
        {row.original.account?.description ?? 'Not Assigned'}
      </div>
    ),
  },
  createFrequencyColumn<Income>('frequencyCount', 'frequency', 'Frequency'),
  createMoneyValueColumn<Income>('amount', 'Amount'),
  createDateColumn<Income>('nextDue', 'Next Due', 'Ongoing', {
    enableSorting: true,
    sortingFn: 'datetime',
  }),
  createDateColumn<Income>('endDate', 'End Date'),
  {
    id: 'actions',
    cell: ({ row }) => {
      const income = row.original;
      return (
        <div className="flex justify-end">
          <IncomeActions income={income} />
        </div>
      );
    },
  },
];

function IncomesTable() {
  const { id: editingId } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const { data: incomesResult, isLoading: incomesLoading } =
    useApiGetAllIncomes();
  const { data: accountsResult, isLoading: accountsLoading } =
    useApiGetAllAccounts();
  const [error, setError] = useState<DisplayError | null>(null);

  const incomes = incomesResult?.success ? incomesResult.value.results : [];
  const accounts = useMemo(
    () => (accountsResult?.success ? accountsResult.value : []),
    [accountsResult],
  );
  const isLoading = incomesLoading || accountsLoading;

  // Get initial account filter from URL
  const initialAccountId = searchParams.get('accountId');

  // Use the shared account filtering hook
  const {
    selectedAccountId,
    setSelectedAccountId,
    filteredItems: filteredIncomes,
  } = useAccountFilter({
    accounts,
    items: incomes,
  });

  // Set initial account filter from URL when data is loaded
  useEffect(() => {
    if (initialAccountId && accounts.length > 0 && !selectedAccountId) {
      // Only set if we have accounts data and haven't already set a filter
      const accountExists = accounts.some(
        account => account.rowId.toString() === initialAccountId,
      );

      if (accountExists) {
        setSelectedAccountId(initialAccountId);
      }
    }
  }, [initialAccountId, accounts, selectedAccountId, setSelectedAccountId]);

  useEffect(() => {
    // Transient error handling - resetting error state when success, such as after a network loss
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
    <>
      <Card className="card-elevated">
        <CardContent className="px-4">
          <DataTable
            columns={columns}
            data={filteredIncomes}
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

export default IncomesTable;
