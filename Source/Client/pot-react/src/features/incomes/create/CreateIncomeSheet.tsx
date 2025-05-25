import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router';

import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import { ErrorSheet } from '@/components/feedback/sheet/ErrorSheet';
import type { Account } from '@/data/accounts/account';
import { CreateIncome } from '@/data/incomes/income';
import { DisplayError } from '@/lib/errors/displayError';
import { Frequency } from '@/lib/types';
import { localToday } from '@/lib/utils';

import { IncomeForm } from '../components/IncomeForm';
import { IncomeSheet } from '../components/IncomeSheet';
import { IncomeFormSchema, incomeFormSchema } from '../schemas/incomeSchema';
import { useCreateIncome } from './hooks/useCreateIncome';

const CreateIncomeSheet = () => {
  const [error, setError] = useState<DisplayError | null>(null);
  const navigate = useNavigate();
  const { createIncome } = useCreateIncome();

  const { data: accountsResult } = useApiGetAllAccounts();

  const accounts: Account[] = accountsResult?.success
    ? accountsResult.value
    : [];

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

  const onSubmit = async (values: IncomeFormSchema) => {
    // if the optional fields are undefined, set them to null to match the CreateIncome type
    const income: CreateIncome = {
      ...values,
      endDate: values.endDate ?? null,
      accountRowId: values.accountRowId ?? null,
    };

    const result = await createIncome(income);

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
    <IncomeSheet title="Create Income">
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
        readOnlyIncomeIdentifiers={false}
        submitLabel="Create"
        accounts={accounts}
      />
    </IncomeSheet>
  );
};

export default CreateIncomeSheet;
