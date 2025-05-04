import { Plus } from 'lucide-react';
import { useEffect, useState } from 'react';

import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import { ErrorDialog, ErrorDialogState } from '@/components/dialog/errorDialog';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import { DataTable } from '@/components/table/DataTable';
import { Button } from '@/components/ui/button';
import { Dialog, DialogTrigger } from '@/components/ui/dialog';

import { columns } from './columns';
import { CreateAccountDialog } from './create/createAccountDialog';

const AccountsPage = () => {
  console.info('Rendering AccountsPage');

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
        <DataTable columns={columns} data={accounts} />
        <LoadingMessage isLoading={isLoading} />
      </div>

      {dialogError && (
        <ErrorDialog
          open={true}
          title={dialogError.title}
          description={dialogError.description}
          onOk={() => setDialogError(null)}
        />
      )}
    </>
  );
};

export default AccountsPage;
