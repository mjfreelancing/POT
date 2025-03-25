import { useQuery } from '@tanstack/react-query';
import { ColumnDef } from '@tanstack/react-table';
import { useEffect,useState } from 'react';

import { getAccounts } from '@/api/accounts/accountsApi';
import { createCurrencyColumn, DataTable } from '@/components/dataTable';
import { Account } from '@/data/accounts/account';

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
  const { data, isLoading, isError } = useQuery({
    queryKey: ['accounts'],
    queryFn: async ({ signal }) => {
      return getAccounts(signal);
    },
  });

  const [showLoading, setShowLoading] = useState(false);

  useEffect(() => {
    let timeout: number;

    if (isLoading) {
      timeout = window.setTimeout(() => setShowLoading(true), 300);
    } else {
      setShowLoading(false);
    }

    return () => clearTimeout(timeout);
  }, [isLoading]);

  return (
    <div className="container mx-auto py-4 px-4">
      {isError && (
        <div className="flex flex-col items-center justify-center text-red-500 mb-8">
          Failed to load accounts
        </div>
      )}

      <DataTable columns={columns} data={data ?? []} />

      {showLoading && (
        <div className="flex flex-col items-center justify-center mt-8">
          Loading...
        </div>
      )}
    </div>
  );
};

export default AccountsPage;
