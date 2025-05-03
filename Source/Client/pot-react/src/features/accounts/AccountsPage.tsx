import { Plus } from 'lucide-react';

import { useGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import { DataTable } from '@/components/ui/DataTable';
import ErrorMessage from '@/components/ui/ErrorMessage';
import LoadingMessage from '@/components/ui/LoadingMessage';
import { Button } from '@/components/ui/shadcn/button';
import { Dialog, DialogTrigger } from '@/components/ui/shadcn/dialog';

import { columns } from './columns';
import { CreateAccountDialog } from './create/createAccountDialog';

const AccountsPage = () => {
  console.info('Rendering AccountsPage');

  const { data, isLoading, isError } = useGetAllAccounts();

  // See how this compares to the loading...
  // https://app.studyraid.com/en/read/11919/379833/component-lazy-loading

  return (
    <div className="container mx-auto py-4 px-4">
      {isError && <ErrorMessage message="Failed to load accounts" />}
      <div className="mb-4">
        <Dialog>
          <DialogTrigger asChild>
            <Button aria-label="Create new account">
              <Plus className="mr-2 h-4 w-4" />
              Add Account
            </Button>
          </DialogTrigger>
          <CreateAccountDialog />
        </Dialog>
      </div>
      <DataTable columns={columns} data={data || []} />
      <LoadingMessage isLoading={isLoading} text="Loading..." />
    </div>
  );
};

export default AccountsPage;
