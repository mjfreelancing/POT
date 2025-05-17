import { Plus } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Outlet, useNavigate, useParams } from 'react-router';

import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import { ErrorSheet } from '@/components/feedback/sheet/ErrorSheet';
import { DataTable } from '@/components/table/DataTable';
import { Button } from '@/components/ui/button';
import { DisplayError } from '@/lib/errors/displayError';

import { columns } from './columns';

const AccountsPage = () => {
  console.info('Rendering AccountsPage');

  const navigate = useNavigate();
  const { id: editingId } = useParams<{ id: string }>();
  const { data: result, isLoading } = useApiGetAllAccounts();
  const [error, setError] = useState<DisplayError | null>(null);

  const accounts = result?.success ? result.value : [];

  useEffect(() => {
    // Transient error handling
    if (result) {
      // Reset error state whe success, such as after a network loss
      setError(
        result.success
          ? null
          : {
              title: result.error.code,
              description: result.error.description,
            },
      );
    }
  }, [result]);

  return (
    <>
      <div className="container mx-auto py-4 px-4">
        <div className="mb-4">
          <Button
            onClick={() => navigate('create')}
            aria-label="Create new account"
          >
            <Plus className="mr-2 h-4 w-4" />
            Add Account
          </Button>
        </div>
        <DataTable
          columns={columns}
          data={accounts}
          highlightRowFilter={row =>
            row.original.rowId.toString() === editingId
          }
        />
        <LoadingMessage isLoading={isLoading} />
      </div>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
      <Outlet />
    </>
  );
};

export default AccountsPage;
