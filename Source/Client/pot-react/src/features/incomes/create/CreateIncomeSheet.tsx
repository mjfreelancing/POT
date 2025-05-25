import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router';

import { ErrorSheet } from '@/components/feedback/sheet/ErrorSheet';
import { CreateIncome } from '@/data/incomes/income';
import { DisplayError } from '@/lib/errors/displayError';
import { localToday } from '@/lib/utils';
import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import { Frequency } from '@/lib/types';

import { IncomeForm } from '../components/IncomeForm';
import { IncomeSheet } from '../components/IncomeSheet';
import { IncomeFormSchema, incomeFormSchema } from '../schemas/incomeSchema';
import { useCreateIncome } from './hooks/useCreateIncome';
import type { Account } from '@/data/accounts/account';

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
      endDate: null,
      frequency: Frequency.Months,
      frequencyCount: 1,
      amount: 0,
      accountRowId: null,
    },
  });

  const onSubmit = async (values: IncomeFormSchema) => {
    const income: CreateIncome = values;
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
