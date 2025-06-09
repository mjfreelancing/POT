import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router';

import { useApiGetAllAccounts, useApiGetAllIncomes } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import AccountFilter from '@/components/filters/AccountFilter';
import { DataTable } from '@/components/table';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { DisplayError } from '@/lib/errors/displayError';
import { useAccountFilter } from '@/hooks/useAccountFilter';

import { columns } from './columns';

function IncomesTable() {
  const { id: editingId } = useParams<{ id: string }>();
  const { data: incomesResult, isLoading: incomesLoading } =
    useApiGetAllIncomes();
  const { data: accountsResult, isLoading: accountsLoading } =
    useApiGetAllAccounts();
  const [error, setError] = useState<DisplayError | null>(null);
  const navigate = useNavigate();

  const incomes = incomesResult?.success ? incomesResult.value.results : [];
  const accounts = accountsResult?.success ? accountsResult.value : [];
  const isLoading = incomesLoading || accountsLoading;

  // Use the shared account filtering hook
  const {
    accountsInItems: accountsInIncomes,
    selectedAccountId,
    setSelectedAccountId,
    filteredItems: filteredIncomes,
  } = useAccountFilter({
    accounts,
    items: incomes,
  });

  useEffect(() => {
    // Transient error handling - resetting error state when success, such as after a network loss
    if (incomesResult) {
      setError(
        incomesResult.success
          ? null
          : {
              title: incomesResult.error.code,
              description: incomesResult.error.description,
            },
      );
    }
  }, [incomesResult]);

  return (
    <>
      <Card>
        <CardHeader className="flex flex-row items-center">
          <div>
            <CardTitle>Income Sources</CardTitle>
            <CardDescription>Manage your income sources.</CardDescription>
          </div>
          {/*
           * ml-auto pushes the element as far right as it can go in a flex container
           * flex makes it a flex container
           * items-center vertically centers its children
           * space-x-4 adds a 1rem horizontal gap between each child
           */}
          <div className="ml-auto flex items-center space-x-4">
            <AccountFilter
              accounts={accountsInIncomes}
              selectedAccountId={selectedAccountId}
              onAccountChange={setSelectedAccountId}
            />
            <Button
              onClick={() => navigate('create')}
              aria-label="Add a new income source"
            >
              Add Income
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={filteredIncomes}
            highlightRowFilter={row =>
              row.original.rowId.toString() === editingId
            }
          />
        </CardContent>
      </Card>
      <LoadingMessage isLoading={isLoading} />
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
    </>
  );
}

export default IncomesTable;
