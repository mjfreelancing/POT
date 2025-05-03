import { ColumnDef } from '@tanstack/react-table';

import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import ErrorMessage from '@/components/feedback/message/ErrorMessage';
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
  const { data, isLoading, isError } = useApiGetAllAccounts();

  return (
    <div className="container mx-auto py-4 px-4">
      {isError && <ErrorMessage message="Failed to load accounts" />}

      <DataTable columns={columns} data={data || []} />

      <DatePickerWithYear />

      <LoadingMessage isLoading={isLoading} text="Loading..." />
    </div>
  );
};

export default DashboardPage;
