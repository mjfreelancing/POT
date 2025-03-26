import { useQuery } from '@tanstack/react-query';
import { ColumnDef } from '@tanstack/react-table';

import { getAccounts } from '@/api/accounts/accountsApi';
import { createCurrencyColumn, DataTable } from '@/components/DataTable';
import { Account } from '@/data/accounts/account';
import LoadingMessage from '@/components/LoadingMessage';
import ErrorMessage from '@/components/ErrorMessage';
import { logFunction } from '@/lib/loggerUtils';

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
  createCurrencyColumn<Account>('reserved', 'Reserved'),
  createCurrencyColumn<Account>('allocated', 'Allocated'),
  createCurrencyColumn<Account>('dailyAccrual', 'Daily Accrual'),
  createCurrencyColumn<Account>('available', 'Available'),
];

const AccountsPage = () => {
  logFunction(AccountsPage);

  const { data, isLoading, isError } = useQuery({
    queryKey: ['accounts'],
    queryFn: async ({ signal }) => {
      return getAccounts(signal);
    },
    retryDelay: 10000,
  });

  return (
    <div className="container mx-auto py-4 px-4">
      {isError && <ErrorMessage message="Failed to load accounts" />}

      <DataTable columns={columns} data={data || []} />

      <LoadingMessage isLoading={isLoading} text="Loading..." />
    </div>
  );
};

export default AccountsPage;
