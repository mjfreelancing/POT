import { ColumnDef } from '@tanstack/react-table';

import { useAllAccountsQuery } from '@/api/accounts/accountsApi';
import {
  createCurrencyColumn,
  DataTable,
} from '@/components/ui-custom/DataTable';
import ErrorMessage from '@/components/ui-custom/ErrorMessage';
import LoadingMessage from '@/components/ui-custom/LoadingMessage';
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
  createCurrencyColumn<Account>('balance', 'Balance'),
  createCurrencyColumn<Account>('available', 'Available'),
];

const DashboardPage = () => {
  const { data, isLoading, isError } = useAllAccountsQuery();

  return (
    <div className="container mx-auto py-4 px-4">
      {isError && <ErrorMessage message="Failed to load accounts" />}

      <DataTable columns={columns} data={data || []} />

      <LoadingMessage isLoading={isLoading} text="Loading..." />
    </div>
  );
};

export default DashboardPage;
