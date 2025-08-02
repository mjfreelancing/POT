import { useQueryClient } from '@tanstack/react-query';
import { ColumnDef, Row } from '@tanstack/react-table';
import { useState } from 'react';
import { useParams } from 'react-router';

import { useApiRenewIncomes } from '@/api/hooks';
import { ErrorSheet, NotePopover } from '@/components/feedback';
import {
  BulkAction,
  createDateColumn,
  createFrequencyColumn,
  createMoneyValueColumn,
  createRowIdGetter,
  DataTable,
  DataTableColumnHeader,
} from '@/components/table';
import { Card, CardContent } from '@/components/ui/card';
import { Income } from '@/data';
import { DisplayError, Frequency } from '@/lib';

import { renewAllIncomes } from '../utils/bulkActions';
import IncomeActions from './IncomeActions';

type IncomesTableProps = {
  filteredIncomes: Income[];
};

const columns: ColumnDef<Income>[] = [
  {
    accessorKey: 'description',
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Description" />
    ),
    enableSorting: true,
    sortingFn: 'text',
    cell: ({ row }) => (
      <div className="flex items-center gap-1">
        {row.original.description}
        <NotePopover note={row.original.note} />
      </div>
    ),
  },
  createMoneyValueColumn<Income>('amount', 'Amount'),
  createDateColumn<Income>('nextDue', 'Next Due', {
    enableSorting: true,
    sortingFn: 'datetime',
  }),
  createFrequencyColumn<Income>('frequencyCount', 'frequency', 'Frequency'),
  createDateColumn<Income>('endDate', 'End Date', {
    getNullValue: row =>
      row.original.frequency === Frequency.OneTime ? undefined : 'Ongoing',
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
      const income = row.original;
      return (
        <div className="flex justify-end">
          <IncomeActions income={income} />
        </div>
      );
    },
  },
];

function IncomesTable({ filteredIncomes }: IncomesTableProps) {
  const { id: editingId } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const renewIncomesMutation = useApiRenewIncomes();
  const [error, setError] = useState<DisplayError | null>(null);

  const bulkActions: BulkAction<Income>[] = [
    {
      label: 'Auto Renew',
      onClick: async (selectedItems: Income[]) => {
        const result = await renewAllIncomes(
          selectedItems,
          renewIncomesMutation,
          queryClient,
        );

        if (!result.success) {
          setError(result.error);
        }
      },
    },
  ];

  return (
    <>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
      <Card className="card-elevated flex flex-col flex-1 min-h-0">
        <CardContent className="px-4 flex-1 min-h-0 flex flex-col">
          <DataTable
            columns={columns}
            data={filteredIncomes}
            enableRowSelection={true}
            bulkActions={bulkActions}
            getRowId={createRowIdGetter<Income>()}
            highlightRowFilter={(row: Row<Income>) =>
              row.original.rowId.toString() === editingId
            }
          />
        </CardContent>
      </Card>
    </>
  );
}

export default IncomesTable;
export type { IncomesTableProps };
