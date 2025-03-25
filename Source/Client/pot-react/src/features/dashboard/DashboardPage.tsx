import { useQuery } from '@tanstack/react-query';

import { getAccounts } from '@/api/accounts/accountsApi';

// Temporary - a WIP

const DashboardPage = () => {
  const {
    data: accounts,
    isLoading,
    isError,
  } = useQuery({
    queryKey: ['accounts'],
    queryFn: async ({ signal }) => {
      return getAccounts(signal);
    },
  });

  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center">
        Loading...
      </div>
    );
  }

  if (isError) {
    return (
      <div className="flex flex-col items-center justify-center text-red-500">
        Failed to load accounts
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center justify-center h-full w-full">
      <h1 className="text-2xl font-bold mb-4 text-foreground">Accounts</h1>
      <ul className="w-1/2 bg-background shadow-md rounded-lg p-4">
        {accounts?.map((account, index) => (
          <li
            key={index}
            className="p-2 border-b last:border-none text-foreground"
          >
            <span className="font-semibold">Id:</span> {account.rowId}
            <br />
            <span className="font-semibold">ETag:</span> {account.eTag}
            <br />
            <span className="font-semibold">BSB:</span> {account.bsb}
            <br />
            <span className="font-semibold">Number:</span> {account.number}
            <br />
            <span className="font-semibold">Description:</span>{' '}
            {account.description}
            <br />
            <span className="font-semibold">Balance:</span> {account.balance}
            <br />
            <span className="font-semibold">Reserved:</span> {account.reserved}
            <br />
            <span className="font-semibold">Allocated:</span>{' '}
            {account.allocated}
            <br />
            <span className="font-semibold">Daily Accrual:</span>{' '}
            {account.dailyAccrual}
            <br />
            <span className="font-semibold">Available:</span>
            {account.available}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default DashboardPage;
