import { Plus } from 'lucide-react';

import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import ErrorMessage from '@/components/feedback/message/ErrorMessage';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import { DataTable } from '@/components/table/DataTable';
import { Button } from '@/components/ui/button';
import { Dialog, DialogTrigger } from '@/components/ui/dialog';

import { columns } from './columns';
import { CreateAccountDialog } from './create/createAccountDialog';

const AccountsPage = () => {
  console.info('Rendering AccountsPage');

  const { data, isLoading, isError } = useApiGetAllAccounts();

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
      <LoadingMessage isLoading={isLoading} />
    </div>
  );
};

export default AccountsPage;
