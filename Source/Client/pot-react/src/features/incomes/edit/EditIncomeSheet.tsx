import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router';

import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import { useApiGetIncomeById } from '@/api/incomes/hooks/useIncomes';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import { ErrorSheet } from '@/components/feedback/sheet/ErrorSheet';
import { Sheet, SheetContent } from '@/components/ui/sheet';
import type { EditIncome } from '@/data/incomes/income';
import type { DisplayError } from '@/lib/errors/displayError';
import { Frequency } from '@/lib/types';
import { localToday } from '@/lib/utils';

import { IncomeForm } from '../components/IncomeForm';
import { IncomeSheet } from '../components/IncomeSheet';
import { IncomeFormData, incomeFormSchema } from '../schemas/incomeFormSchema';
import { useEditIncome } from './hooks/useEditIncome';

const EditIncomeSheet = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: incomeResult, isLoading: isIncomeLoading } =
    useApiGetIncomeById(id!);
  const { data: accountsResult, isLoading: isAccountsLoading } =
    useApiGetAllAccounts();
  const { editIncome } = useEditIncome();

  const [error, setError] = useState<DisplayError | null>(null);
  const [initialized, setInitialized] = useState(false);

  const form = useForm<IncomeFormData>({
    resolver: zodResolver(incomeFormSchema),
    mode: 'onSubmit',
    defaultValues: {
      description: '',
      nextDue: localToday(),
      endDate: undefined,
      frequency: Frequency.Months,
      frequencyCount: 1,
      amount: 0,
      accountRowId: undefined, // undefined since the <Select> component on <IncomeForm> expects string | undefined
    },
  });

  useEffect(() => {
    if (!initialized && incomeResult?.success && accountsResult?.success) {
      const income = incomeResult.value;

      form.reset({
        description: income.description,
        nextDue: income.nextDue,
        endDate: income.endDate ?? undefined, // if endDate is null, set it to undefined to match the form's expected type
        frequency: income.frequency,
        frequencyCount: income.frequencyCount,
        amount: income.amount,
        accountRowId: income.account?.rowId,
      });
      setInitialized(true);
    }
  }, [initialized, incomeResult, accountsResult, form]);

  // Show loading state until both income and account lists arrive
  // initialized is required to ensure the form is only shown once
  // the initial form reset has completed. If this is not done then
  // the Frequency and Associated Account fields will not be set.
  if (isIncomeLoading || isAccountsLoading || !initialized) {
    return (
      <IncomeSheet title="Edit Income">
        <LoadingMessage isLoading={true} />
      </IncomeSheet>
    );
  }

  const renderErrorSheet = (title: string, description: string) => (
    <Sheet open={true}>
      <SheetContent>
        <ErrorSheet
          title={title}
          description={description}
          onDismiss={() => navigate('/incomes')}
        />
      </SheetContent>
    </Sheet>
  );

  // Deal with failure to load income results
  if (incomeResult && !incomeResult.success) {
    return renderErrorSheet(
      incomeResult.error.code,
      incomeResult.error.description,
    );
  }

  // Deal with failure to load accounts
  if (accountsResult && !accountsResult.success) {
    return renderErrorSheet(
      accountsResult.error.code,
      accountsResult.error.description,
    );
  }

  const { rowId, eTag } = incomeResult.value;
  const accounts = accountsResult.value;

  const onSubmit = async (values: IncomeFormData) => {
    const payload: EditIncome = {
      rowId,
      eTag,
      ...values,
      endDate: values.endDate ?? null,
      accountRowId: values.accountRowId ?? null,
    };

    const result = await editIncome(payload);

    if (result.success) {
      navigate('/incomes');
    } else {
      setError({
        title: result.error.code,
        description: result.error.description,
      });
    }
  };

  return (
    <IncomeSheet title="Edit Income">
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
      <IncomeForm
        form={form}
        onSubmit={onSubmit}
        onCancel={() => navigate('/incomes')}
        readOnlyIncomeIdentifiers={true}
        submitLabel="Save"
        accounts={accounts}
      />
    </IncomeSheet>
  );
};

export default EditIncomeSheet;
