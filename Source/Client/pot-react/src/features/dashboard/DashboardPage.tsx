import { ColumnDef } from '@tanstack/react-table';
import { useEffect, useState } from 'react';

import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import { ErrorSheet } from '@/components/feedback/sheet/ErrorSheet';
import { DatePickerWithYear } from '@/components/picker/DatePickerWithYear';
import {
  createMoneyValueColumn,
  DataTable,
} from '@/components/table/DataTable';
import { Account } from '@/data/accounts/account';
import { DisplayError } from '@/lib/errors/displayError';

// Temporary - a WIP

const columns: ColumnDef<Account>[] = [
  {
    accessorKey: 'bsb_number',
    header: 'BSB / Number',
    cell: ({ row }) => {
      const { bsb, number } = row.original;
      return `(${bsb}) ${number}`;
    },
  },
  {
    accessorKey: 'description',
    header: 'Description',
  },
  createMoneyValueColumn<Account>('balance', 'Balance'),
  createMoneyValueColumn<Account>('available', 'Available'),
];

const DashboardPage = () => {
  const [error, setError] = useState<DisplayError | null>(null);
  const { data: result, isLoading } = useApiGetAllAccounts();

  useEffect(() => {
    // Transient error handling
    if (result) {
      if (result.success) {
        // Reset error state, such as after a network loss
        setError(null);
      } else {
        setError({
          title: result.error.code,
          description: result.error.description,
        });
      }
    }
  }, [result]);

  const accounts = result?.success ? result.value : [];

  return (
    <>
      <div className="container mx-auto py-4 px-4">
        <DataTable columns={columns} data={accounts} />

        <DatePickerWithYear />

        <LoadingMessage isLoading={isLoading} />
      </div>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
    </>
  );
};

export default DashboardPage;
