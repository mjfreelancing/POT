import { useQueryClient } from '@tanstack/react-query';
import type { ColumnDef, Row } from '@tanstack/react-table';
import { CheckCircle, EyeOff, FastForward } from 'lucide-react';
import { useState } from 'react';
import { useParams } from 'react-router';
import { toast } from 'sonner';

import { useApiRenewIncomes, useApiToggleExcludeIncomes } from '@/api/hooks';
import { ConfirmationDialog } from '@/components/dialog';
import { ErrorSheet, SuccessToast } from '@/components/feedback';
import type { BulkAction } from '@/components/table';
import {
  createAccountDescriptionColumn,
  createActionsColumn,
  createFrequencyColumn,
  createMoneyValueColumn,
  createNextDueStatusColumn,
  createRecurringEndDateColumn,
  createRowIdGetter,
  DataTable,
  DataTableColumnHeader,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import { useErrorContext } from '@/contexts';
import type { Income } from '@/data';
import { usePermissions } from '@/hooks';
import {
  getAdornedIncomeDescription,
  getDaysDue,
  getTableRowClassName,
  RenewalMode,
} from '@/lib';

import { renewIncomes, toggleExcludeIncomes } from '../bulkActions';
import IncomeActions from './IncomeActions';

type IncomesTableProps = {
  filteredIncomes: Income[];
};

const columns: ColumnDef<Income>[] = [
  {
    id: 'description',
    accessorKey: 'description',
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Description" />
    ),
    enableSorting: true,
    sortingFn: 'text',
    cell: ({ row }) => getAdornedIncomeDescription(row),
  },
  createMoneyValueColumn<Income>({
    accessorKey: 'amount',
    header: 'Amount',
    options: {
      enableSorting: true,
      sortingFn: 'basic',
    },
  }),
  createFrequencyColumn<Income>({
    countKey: 'frequencyCount',
    frequencyKey: 'frequency',
    header: 'Frequency',
  }),
  createNextDueStatusColumn<Income>(),
  createRecurringEndDateColumn<Income>(),
  createAccountDescriptionColumn<Income>(),
  createActionsColumn<Income>(income => <IncomeActions income={income} />),
];

function IncomesTable({ filteredIncomes }: IncomesTableProps) {
  const { id: editingId } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const renewIncomesMutation = useApiRenewIncomes();
  const excludeIncomesMutation = useApiToggleExcludeIncomes();
  const { error, setError } = useErrorContext();
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [pendingIncomes, setPendingIncomes] = useState<Income[]>([]);

  const { hasPermission } = usePermissions();
  const canManageIncomes = hasPermission('income:manage');

  async function processMarkAsReceived() {
    const excludedIncomes = pendingIncomes.filter(
      income => income.excludeFromCalcs,
    );

    const actionableIncomes = pendingIncomes.filter(
      income => !income.excludeFromCalcs,
    );

    const futureIncomes = actionableIncomes.filter(
      income => getDaysDue(income.nextDue) > 0,
    );

    const overdueIncomes = actionableIncomes.filter(
      income => getDaysDue(income.nextDue) <= 0,
    );

    let hasErrors = false;

    // Process future incomes (mark as received)
    if (futureIncomes.length > 0) {
      const futureRowIds = futureIncomes.map(item => item.rowId);
      const result = await renewIncomes(
        futureRowIds,
        RenewalMode.Future,
        renewIncomesMutation,
        queryClient,
      );
      if (!result.success) {
        setError(result.error);
        hasErrors = true;
      }
    }

    // Process overdue incomes (advance renewal)
    if (overdueIncomes.length > 0) {
      const overdueRowIds = overdueIncomes.map(item => item.rowId);
      const result = await renewIncomes(
        overdueRowIds,
        RenewalMode.Overdue,
        renewIncomesMutation,
        queryClient,
      );
      if (!result.success) {
        setError(result.error);
        hasErrors = true;
      }
    }

    if (!hasErrors) {
      const messages = [];

      if (futureIncomes.length > 0) {
        messages.push(
          `${futureIncomes.length} income${futureIncomes.length > 1 ? 's' : ''} marked as received`,
        );
      }

      if (overdueIncomes.length > 0) {
        messages.push(
          `${overdueIncomes.length} overdue income${overdueIncomes.length > 1 ? 's' : ''} advanced`,
        );
      }

      if (excludedIncomes.length > 0) {
        messages.push(
          `${excludedIncomes.length} excluded income${excludedIncomes.length > 1 ? 's' : ''} skipped`,
        );
      }
      toast(
        <SuccessToast
          icon={CheckCircle}
          title="Incomes Processed"
          description={messages.join(', ')}
        />,
      );
    }

    setShowConfirmation(false);
    setPendingIncomes([]);
  }

  function getConfirmationMessage(incomes: Income[]): React.ReactNode {
    const excludedCount = incomes.filter(
      income => income.excludeFromCalcs,
    ).length;

    const actionableIncomes = incomes.filter(
      income => !income.excludeFromCalcs,
    );

    const futureCount = actionableIncomes.filter(
      income => getDaysDue(income.nextDue) > 0,
    ).length;

    const overdueCount = actionableIncomes.filter(
      income => getDaysDue(income.nextDue) <= 0,
    ).length;

    return (
      <div className="space-y-3">
        {futureCount > 0 && (
          <div className="flex items-start gap-3">
            <CheckCircle className="h-5 w-5 text-green-600 dark:text-green-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-green-700 dark:text-green-300">
                {futureCount} income{futureCount > 1 ? 's' : ''}
              </span>{' '}
              not yet due will be marked as received early and moved forward to
              the next period.
            </div>
          </div>
        )}
        {overdueCount > 0 && (
          <div className="flex items-start gap-3">
            <FastForward className="h-5 w-5 text-orange-600 dark:text-orange-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-orange-700 dark:text-orange-300">
                {overdueCount} overdue income{overdueCount > 1 ? 's' : ''}
              </span>{' '}
              will be caught up to their next scheduled due date.
            </div>
          </div>
        )}
        {excludedCount > 0 && (
          <div className="flex items-start gap-3">
            <EyeOff className="h-5 w-5 text-slate-600 dark:text-slate-400 flex-shrink-0 mt-0.5" />
            <div>
              <span className="font-bold text-slate-700 dark:text-slate-300">
                {excludedCount} excluded income{excludedCount > 1 ? 's' : ''}
              </span>{' '}
              will be skipped. Excluded items are not updated by this action.
            </div>
          </div>
        )}
        <div className="text-sm text-muted-foreground pt-2 border-t">
          This action cannot be undone.
        </div>
      </div>
    );
  }

  const bulkActions: BulkAction<Income>[] = [
    {
      label: 'Mark as Received',
      isDisabled: !canManageIncomes,
      onClick: async (selectedItems: Income[]) => {
        setPendingIncomes(selectedItems);
        setShowConfirmation(true);
      },
      clearSelectionOnComplete: true,
    },
    {
      label: 'Toggle Exclusion',
      isDisabled: !canManageIncomes,
      onClick: async (selectedItems: Income[]) => {
        const result = await toggleExcludeIncomes(
          selectedItems,
          excludeIncomesMutation,
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
        title="Mark Incomes as Received"
        description={getConfirmationMessage(pendingIncomes)}
        confirmLabel="Proceed"
        cancelLabel="Cancel"
        onConfirm={processMarkAsReceived}
        onCancel={() => {
          setShowConfirmation(false);
          setPendingIncomes([]);
        }}
      />

      <Card className="card-elevated flex flex-col flex-1 min-h-0">
        <CardContent className="px-4 flex-1 min-h-0 flex flex-col">
          <DataTable
            columns={columns}
            data={filteredIncomes}
            enableRowSelection={hasAnyBulkPermission}
            bulkActions={bulkActions}
            getRowId={createRowIdGetter<Income>()}
            highlightRowFilter={(row: Row<Income>) =>
              row.original.rowId.toString() === editingId
            }
            getRowClassName={(row: Row<Income>) =>
              getTableRowClassName(row.original, { exclude: ['OVERDUE'] })
            }
          />
        </CardContent>
      </Card>
    </>
  );
}

export default IncomesTable;
export type { IncomesTableProps };
