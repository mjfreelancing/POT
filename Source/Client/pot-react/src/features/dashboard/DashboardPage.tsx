import { ColumnDef } from '@tanstack/react-table';
import { useEffect, useState } from 'react';

import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import { ErrorDialog, ErrorDialogState } from '@/components/dialog/errorDialog';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import { DatePickerWithYear } from '@/components/picker/DatePickerWithYear';
import {
  createMoneyValueColumn,
  DataTable,
} from '@/components/table/DataTable';
import { Account } from '@/data/accounts/account';

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
  const [dialogError, setDialogError] = useState<ErrorDialogState | null>(null);
  const { data: result, isLoading } = useApiGetAllAccounts();

  useEffect(() => {
    if (result && !result.success) {
      setDialogError({
        title: result.error.code,
        description: result.error.description,
      });
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
      {dialogError && (
        <ErrorDialog
          open={true}
          title={dialogError.title}
          description={dialogError.description}
          onOk={() => setDialogError(null)}
        />
      )}{' '}
    </>
  );
};

export default DashboardPage;
