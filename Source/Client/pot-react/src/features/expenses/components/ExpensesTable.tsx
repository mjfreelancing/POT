import { ColumnDef } from '@tanstack/react-table';
import { useEffect, useMemo, useState } from 'react';
import { useParams, useSearchParams } from 'react-router';

import { useApiGetAllAccounts, useApiGetAllExpenses } from '@/api/hooks';
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
import { Expense } from '@/data';
import { useAccountFilter } from '@/hooks';
import { DisplayError } from '@/lib';

import ExpenseActions from './ExpenseActions';

const columns: ColumnDef<Expense>[] = [
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
  createFrequencyColumn<Expense>('frequencyCount', 'frequency', 'Frequency'),
  {
    accessorKey: 'recurring',
    header: 'Recurring',
    cell: ({ row }) => <div>{row.original.recurring ? 'Yes' : 'No'}</div>,
  },
  createMoneyValueColumn<Expense>('amount', 'Amount'),
  createDateColumn<Expense>('accrualStart', 'Accrual Start'),
  createDateColumn<Expense>('nextDue', 'Next Due', 'Ongoing', {
    enableSorting: true,
    sortingFn: 'datetime',
  }),
  createDateColumn<Expense>('endDate', 'End Date'),
  {
    id: 'actions',
    cell: ({ row }) => {
      const expense = row.original;
      return (
        <div className="flex justify-end">
          <ExpenseActions expense={expense} />
        </div>
      );
    },
  },
];

function ExpensesTable() {
  const { id: editingId } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const { data: expensesResult, isLoading: expensesLoading } =
    useApiGetAllExpenses();
  const { data: accountsResult, isLoading: accountsLoading } =
    useApiGetAllAccounts();
  const [error, setError] = useState<DisplayError | null>(null);

  const expenses = expensesResult?.success ? expensesResult.value.results : [];
  const accounts = useMemo(
    () => (accountsResult?.success ? accountsResult.value : []),
    [accountsResult],
  );
  const isLoading = expensesLoading || accountsLoading;

  // Get initial account filter from URL
  const initialAccountId = searchParams.get('accountId');

  // Use the shared account filtering hook
  const {
    selectedAccountId,
    setSelectedAccountId,
    filteredItems: filteredExpenses,
  } = useAccountFilter({
    accounts,
    items: expenses,
  });

  // Set initial account filter from URL when data is loaded
  useEffect(() => {
    if (initialAccountId && accounts.length > 0) {
      // Only set if we have accounts data
      const accountExists = accounts.some(
        account => account.rowId.toString() === initialAccountId,
      );

      if (accountExists && selectedAccountId !== initialAccountId) {
        setSelectedAccountId(initialAccountId);
      }
    } else if (!initialAccountId && selectedAccountId) {
      // Clear the filter if there's no accountId in URL but we have a selection
      setSelectedAccountId(null);
    }
  }, [initialAccountId, accounts, selectedAccountId, setSelectedAccountId]);

  useEffect(() => {
    // Transient error handling - resetting error state when success, such as after a network loss
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

  return (
    <>
      <Card className="card-elevated">
        <CardContent className="px-4">
          <DataTable
            columns={columns}
            data={filteredExpenses}
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

export default ExpensesTable;
