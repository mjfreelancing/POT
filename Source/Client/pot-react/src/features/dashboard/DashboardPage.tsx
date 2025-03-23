import { getAccounts } from '@/api/accounts/accountsApi';
import { useQuery } from '@tanstack/react-query';

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
            <span className="font-semibold">BSB:</span> {account.bsb}
            <br />
            <span className="font-semibold">Number:</span> {account.number}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default DashboardPage;
