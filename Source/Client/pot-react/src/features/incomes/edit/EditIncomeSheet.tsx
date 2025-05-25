import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router';
import { zodResolver } from '@hookform/resolvers/zod';

import { useApiGetIncomeById } from '@/api/incomes/hooks/useIncomes';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import { ErrorSheet } from '@/components/feedback/sheet/ErrorSheet';
import { IncomeSheet } from '../components/IncomeSheet';
import { IncomeForm } from '../components/IncomeForm';
import { incomeFormSchema, IncomeFormSchema } from '../schemas/incomeSchema';
import { useEditIncome } from './hooks/useEditIncome';
import type { EditIncome } from '@/data/incomes/income';
import type { DisplayError } from '@/lib/errors/displayError';
import { localToday } from '@/lib/utils';
import { Frequency } from '@/lib/types';
import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import { Sheet, SheetContent } from '@/components/ui/sheet';

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

  const form = useForm<IncomeFormSchema>({
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
        endDate: income.endDate,
        frequency: income.frequency,
        frequencyCount: income.frequencyCount,
        amount: income.amount,
        accountRowId: income.account?.rowId, //mjf ?? '',
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

  // Deal with failure to load data
  if (incomeResult && !incomeResult.success) {
    return (
      <Sheet open={true}>
        <SheetContent>
          <ErrorSheet
            title={incomeResult.error.code}
            description={incomeResult.error.description}
            onDismiss={() => navigate('/incomes')}
          />
        </SheetContent>
      </Sheet>
    );
  }

  // Deal with failure to load data
  if (accountsResult && !accountsResult.success) {
    return (
      <Sheet open={true}>
        <SheetContent>
          <ErrorSheet
            title={accountsResult.error.code}
            description={accountsResult.error.description}
            onDismiss={() => navigate('/incomes')}
          />
        </SheetContent>
      </Sheet>
    );
  }

  const accounts = accountsResult.value;
  const { rowId, eTag } = incomeResult.value;

  const onSubmit = async (values: IncomeFormSchema) => {
    const payload: EditIncome = {
      rowId,
      eTag,
      description: values.description,
      nextDue: values.nextDue,
      endDate: values.endDate,
      frequency: values.frequency,
      frequencyCount: values.frequencyCount,
      amount: values.amount,
      accountRowId: values.accountRowId,
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
