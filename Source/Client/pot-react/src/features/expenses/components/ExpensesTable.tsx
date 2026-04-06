import { useQueryClient } from '@tanstack/react-query';
import type { ColumnDef, Row } from '@tanstack/react-table';
import { CheckCircle, FastForward } from 'lucide-react';
import { useState } from 'react';
import { useParams } from 'react-router';
import { toast } from 'sonner';

import {
  useApiRenewExpenses,
  useApiToggleExcludeExpenses,
} from '@/api/hooks/useExpenses';
import { ConfirmationDialog } from '@/components/dialog';
import { ErrorSheet, SuccessToast } from '@/components/feedback';
import type { BulkAction } from '@/components/table';
import {
  createDateColumn,
  createFrequencyColumn,
  createMoneyValueColumn,
  createRowIdGetter,
  DataTable,
  DataTableColumnHeader,
} from '@/components/table';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { useErrorContext } from '@/contexts';
import type { Expense } from '@/data';
import { usePermissions } from '@/hooks';
import {
  AccrualPolicy,
  formatDate,
  formatMoneyValue,
  Frequency,
  getAdornedExpenseDescription,
  getDaysDue,
  getStatusBadgeClass,
  getTableBadgeClass,
  getTableRowClassName,
  RenewalMode,
} from '@/lib';

import { renewExpenses, toggleExcludeExpenses } from '../bulkActions';
import ExpenseActions from './ExpenseActions';

const columns: ColumnDef<Expense>[] = [
  {
    id: 'description',
    accessorKey: 'description',
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Description" />
    ),
    enableSorting: true,
    sortingFn: 'text',
    cell: ({ row }) => getAdornedExpenseDescription(row),
  },
  createMoneyValueColumn<Expense>({
    accessorKey: 'amount',
    header: 'Amount',
    options: {
      enableSorting: true,
      sortingFn: 'basic',
    },
  }),
  createFrequencyColumn<Expense>({
    countKey: 'frequencyCount',
    frequencyKey: 'frequency',
    header: 'Frequency',
  }),
  {
    id: 'nextDue',
    accessorKey: 'nextDue',
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Next Due" />
    ),
    enableSorting: true,
    sortingFn: 'datetime',
    cell: ({ row }) => {
      const rawValue = row.getValue('nextDue') as string | Date;
      if (!rawValue) return null;

      const formattedDate = formatDate(rawValue);
      const daysDue = getDaysDue(rawValue as string);
      const endDate = row.original.endDate;
      const isEnded = endDate ? getDaysDue(endDate) < 0 : false;
      const isExcluded = row.original.excludeFromCalcs;

      // Determine badge (don't show for excluded items)
      let badge: React.ReactNode = null;
      if (isExcluded) {
        badge = (
          <Badge
            variant="secondary"
            className={getStatusBadgeClass('excluded')}
          >
            Excluded
          </Badge>
        );
      } else if (isEnded) {
        badge = (
          <Badge variant="secondary" className={getStatusBadgeClass('ended')}>
            Ended
          </Badge>
        );
      } else {
        if (daysDue === 0) {
          // Due today - use amber for visibility
          badge = (
            <Badge
              variant="default"
              className={getStatusBadgeClass('due-today')}
            >
              Due Today
            </Badge>
          );
        } else if (daysDue < 0) {
          // Overdue - use bright red for visibility
          badge = (
            <Badge
              variant="destructive"
              className={getStatusBadgeClass('overdue')}
            >
              Overdue
            </Badge>
          );
        } else if (daysDue <= 7) {
          // Due soon (within 7 days)
          badge = (
            <Badge
              variant="default"
              className={getStatusBadgeClass('due-soon')}
            >
              Due Soon
            </Badge>
          );
        }
      }

      return (
        <div className="flex items-center">
          <span className="min-w-[80px] inline-block">{formattedDate}</span>
          {badge}
        </div>
      );
    },
  },
  {
    id: 'endDate',
    accessorKey: 'endDate',
    header: 'End Date',
    cell: ({ row }) => {
      const endDate = row.original.endDate;
      const isOneTime = row.original.frequency === Frequency.OneTime;
      const isExcluded = row.original.excludeFromCalcs;

      if (isOneTime) {
        return (
          <Badge
            variant="secondary"
            className={getTableBadgeClass(
              isExcluded ? 'slate' : 'pink',
              isExcluded ? 'filled' : 'outline',
            )}
          >
            One-time
          </Badge>
        );
      }

      if (!endDate) {
        return null;
      }

      return <span>{formatDate(endDate)}</span>;
    },
  },
  createDateColumn<Expense>({
    accessorKey: 'accrualStart',
    header: 'Accrual Start',
  }),
  createMoneyValueColumn<Expense>({
    accessorKey: 'accrued',
    header: 'Accrued',
    options: {
      cell: ({ row }) => {
        if (row.original.accrualPolicy === AccrualPolicy.None) {
          return null;
        }

        return (
          <span className="min-w-[80px] inline-block">
            {formatMoneyValue(row.original.accrued)}
          </span>
        );
      },
      enableSorting: true,
      sortingFn: 'basic',
    },
  }),
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

type ExpensesTableProps = {
  filteredExpenses: Expense[];
};

function ExpensesTable({ filteredExpenses }: ExpensesTableProps) {
  const { id: editingId } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const renewExpensesMutation = useApiRenewExpenses();
  const excludeExpensesMutation = useApiToggleExcludeExpenses();
  const { error, setError } = useErrorContext();
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [pendingExpenses, setPendingExpenses] = useState<Expense[]>([]);

  const { hasPermission } = usePermissions();
  const canManageExpenses = hasPermission('expense:manage');

  async function processMarkAsPaid() {
    const futureExpenses = pendingExpenses.filter(
      expense => getDaysDue(expense.nextDue) > 0,
    );
    const overdueExpenses = pendingExpenses.filter(
      expense => getDaysDue(expense.nextDue) <= 0,
    );

    let hasErrors = false;

    // Process future expenses (mark as paid)
    if (futureExpenses.length > 0) {
      const futureRowIds = futureExpenses.map(item => item.rowId);
      const result = await renewExpenses(
        futureRowIds,
        RenewalMode.Future,
        renewExpensesMutation,
        queryClient,
      );
      if (!result.success) {
        setError(result.error);
        hasErrors = true;
      }
    }

    // Process overdue expenses (advance renewal)
    if (overdueExpenses.length > 0) {
      const overdueRowIds = overdueExpenses.map(item => item.rowId);
      const result = await renewExpenses(
        overdueRowIds,
        RenewalMode.Overdue,
        renewExpensesMutation,
        queryClient,
      );
      if (!result.success) {
        setError(result.error);
        hasErrors = true;
      }
    }

    if (!hasErrors) {
      const messages = [];
      if (futureExpenses.length > 0) {
        messages.push(
          `${futureExpenses.length} expense${futureExpenses.length > 1 ? 's' : ''} marked as paid`,
        );
      }
      if (overdueExpenses.length > 0) {
        messages.push(
          `${overdueExpenses.length} overdue expense${overdueExpenses.length > 1 ? 's' : ''} advanced`,
        );
      }
      toast(
        <SuccessToast
          icon={CheckCircle}
          title="Expenses Processed"
          description={messages.join(', ')}
        />,
      );
    }

    setShowConfirmation(false);
    setPendingExpenses([]);
  }

  function getConfirmationMessage(expenses: Expense[]): React.ReactNode {
    const futureCount = expenses.filter(
      expense => getDaysDue(expense.nextDue) > 0,
    ).length;
    const overdueCount = expenses.filter(
      expense => getDaysDue(expense.nextDue) <= 0,
    ).length;

    return (
      <div className="space-y-3">
        {futureCount > 0 && (
          <div className="flex items-start gap-3">
            <CheckCircle className="h-5 w-5 text-green-600 dark:text-green-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-green-700 dark:text-green-300">
                {futureCount} expense{futureCount > 1 ? 's' : ''}
              </span>{' '}
              not yet due will be marked as paid early and moved forward to the
              next period.
            </div>
          </div>
        )}
        {overdueCount > 0 && (
          <div className="flex items-start gap-3">
            <FastForward className="h-5 w-5 text-red-600 dark:text-red-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-red-700 dark:text-red-300">
                {overdueCount} overdue expense{overdueCount > 1 ? 's' : ''}
              </span>{' '}
              will be caught up to their next scheduled due date.
            </div>
          </div>
        )}
        <div className="text-sm text-muted-foreground pt-2 border-t">
          This action cannot be undone.
        </div>
      </div>
    );
  }

  const bulkActions: BulkAction<Expense>[] = [
    {
      label: 'Mark as Paid',
      isDisabled: !canManageExpenses,
      onClick: async (selectedItems: Expense[]) => {
        setPendingExpenses(selectedItems);
        setShowConfirmation(true);
      },
      clearSelectionOnComplete: true,
    },
    {
      label: 'Toggle Exclusion',
      isDisabled: !canManageExpenses,
      onClick: async (selectedItems: Expense[]) => {
        const result = await toggleExcludeExpenses(
          selectedItems,
          excludeExpensesMutation,
          queryClient,
        );

        if (!result.success) {
          setError(result.error);
        }
      },
    },
  ];

  // Enable multi-select only if user has permission for any bulk action
  // (isDisabled is based on permissions within each bulk action)
  const hasAnyBulkPermission = bulkActions.some(action => !action.isDisabled);

  return (
    <>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}

      <ConfirmationDialog
        open={showConfirmation}
        title="Mark Expenses as Paid"
        description={getConfirmationMessage(pendingExpenses)}
        confirmLabel="Proceed"
        cancelLabel="Cancel"
        onConfirm={processMarkAsPaid}
        onCancel={() => {
          setShowConfirmation(false);
          setPendingExpenses([]);
        }}
      />

      <Card className="card-elevated flex flex-col flex-1 min-h-0">
        <CardContent className="px-4 flex-1 min-h-0 flex flex-col">
          <DataTable
            columns={columns}
            data={filteredExpenses}
            enableRowSelection={hasAnyBulkPermission}
            bulkActions={bulkActions}
            getRowId={createRowIdGetter<Expense>()}
            highlightRowFilter={(row: Row<Expense>) =>
              row.original.rowId.toString() === editingId
            }
            getRowClassName={(row: Row<Expense>) =>
              getTableRowClassName(row.original, { exclude: ['OVERDUE'] })
            }
          />
        </CardContent>
      </Card>
    </>
  );
}

export default ExpensesTable;
export type { ExpensesTableProps };
