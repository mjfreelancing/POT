import { useAllAccountsQuery } from '@/api/accounts/accountsApi';
import { DataTable } from '@/components/ui/DataTable';
import ErrorMessage from '@/components/ui/ErrorMessage';
import LoadingMessage from '@/components/ui/LoadingMessage';

import { columns } from './columns';

const AccountsPage = () => {
  console.info('Rendering AccountsPage');

  const { data, isLoading, isError } = useAllAccountsQuery();

  return (
    <div className="container mx-auto py-4 px-4">
      {isError && <ErrorMessage message="Failed to load accounts" />}

      <DataTable columns={columns} data={data || []} />

      <LoadingMessage isLoading={isLoading} text="Loading..." />
    </div>
  );
};

export default AccountsPage;
