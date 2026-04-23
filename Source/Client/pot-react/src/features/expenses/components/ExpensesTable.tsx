import { useQueryClient } from '@tanstack/react-query';
import type { ColumnDef, Row } from '@tanstack/react-table';
import { CheckCircle, EyeOff, FastForward } from 'lucide-react';
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
  createAccountDescriptionColumn,
  createActionsColumn,
  createDateColumn,
  createFrequencyColumn,
  createMoneyValueColumn,
  createNextDueStatusColumn,
  createRecurringEndDateColumn,
  createRowIdGetter,
  DataTable,
  DataTableColumnHeader,
  renderMinWidthTableCell,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import { useErrorContext } from '@/contexts';
import type { Expense } from '@/data';
import { usePermissions } from '@/hooks';
import {
  AccrualPolicy,
  formatMoneyValue,
  getAdornedExpenseDescription,
  getTableRowClassName,
  RenewalMode,
} from '@/lib';

import { renewExpenses, toggleExcludeExpenses } from '../bulkActions';
import { splitExpensesByActionability } from '../utils/splitExpensesByActionability';
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
  createNextDueStatusColumn<Expense>(),
  createRecurringEndDateColumn<Expense>(),
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

        return renderMinWidthTableCell(formatMoneyValue(row.original.accrued));
      },
      enableSorting: true,
      sortingFn: 'basic',
    },
  }),
  createAccountDescriptionColumn<Expense>(),
  createActionsColumn<Expense>(expense => <ExpenseActions expense={expense} />),
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
    const {
      excludedExpenses,
      endedExpenses,
      futureExpenses,
      dueTodayExpenses,
      overdueExpenses,
    } = splitExpensesByActionability(pendingExpenses);

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

    // Process due today and overdue expenses (advance renewal)
    if (dueTodayExpenses.length > 0 || overdueExpenses.length > 0) {
      const renewalRowIds = [...dueTodayExpenses, ...overdueExpenses].map(
        item => item.rowId,
      );

      const result = await renewExpenses(
        renewalRowIds,
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

      if (dueTodayExpenses.length > 0) {
        messages.push(
          `${dueTodayExpenses.length} due today expense${dueTodayExpenses.length > 1 ? 's' : ''} processed`,
        );
      }

      if (overdueExpenses.length > 0) {
        messages.push(
          `${overdueExpenses.length} overdue expense${overdueExpenses.length > 1 ? 's' : ''} advanced`,
        );
      }

      if (excludedExpenses.length > 0) {
        messages.push(
          `${excludedExpenses.length} excluded expense${excludedExpenses.length > 1 ? 's' : ''} skipped`,
        );
      }

      if (endedExpenses.length > 0) {
        messages.push(
          `${endedExpenses.length} ended expense${endedExpenses.length > 1 ? 's' : ''} skipped`,
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
    const {
      excludedExpenses,
      endedExpenses,
      futureExpenses,
      dueTodayExpenses,
      overdueExpenses,
    } = splitExpensesByActionability(expenses);

    const excludedCount = excludedExpenses.length;
    const endedCount = endedExpenses.length;
    const futureCount = futureExpenses.length;
    const dueTodayCount = dueTodayExpenses.length;
    const overdueCount = overdueExpenses.length;

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
        {dueTodayCount > 0 && (
          <div className="flex items-start gap-3">
            <CheckCircle className="h-5 w-5 text-amber-600 dark:text-amber-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-amber-700 dark:text-amber-300">
                {dueTodayCount} due today expense{dueTodayCount > 1 ? 's' : ''}
              </span>{' '}
              will be processed and moved forward to the next period.
            </div>
          </div>
        )}
        {excludedCount > 0 && (
          <div className="flex items-start gap-3">
            <EyeOff className="h-5 w-5 text-slate-600 dark:text-slate-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-slate-700 dark:text-slate-300">
                {excludedCount} excluded expense{excludedCount > 1 ? 's' : ''}
              </span>{' '}
              will be skipped. Excluded items are not updated by this action.
            </div>
          </div>
        )}
        {endedCount > 0 && (
          <div className="flex items-start gap-3">
            <EyeOff className="h-5 w-5 text-slate-600 dark:text-slate-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-slate-700 dark:text-slate-300">
                {endedCount} ended expense{endedCount > 1 ? 's' : ''}
              </span>{' '}
              will be skipped. Ended items are not updated by this action.
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
      // Hide this action when all selected items are non-actionable.
      // Excluded or ended items are skipped and would not change due dates.
      isHidden: (selectedItems: Expense[]) => {
        if (selectedItems.length === 0) {
          return false;
        }

        const { actionableExpenses } =
          splitExpensesByActionability(selectedItems);

        return actionableExpenses.length === 0;
      },
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
