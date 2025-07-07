import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router';

import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import type { Account, CreateIncome, Income } from '@/data';
import { DisplayError, Frequency, todayIsoFormat } from '@/lib';

import IncomeForm from '../../components/IncomeForm';
import IncomeSheet from '../../components/IncomeSheet';
import {
  IncomeFormData,
  incomeFormSchema,
} from '../../schemas/incomeFormSchema';
import useCreateIncome from '../hooks/useCreateIncome';

type CreateIncomeFormProps = {
  accountsList: Account[];
  duplicateIncome?: Income;
};

/**
 * Pure form component that handles income creation logic
 * Receives all required data as props - no API calls or loading states
 */
function CreateIncomeForm({
  accountsList,
  duplicateIncome,
}: CreateIncomeFormProps) {
  const navigate = useNavigate();
  const { createIncome } = useCreateIncome();

  const getDefaultValues = (): IncomeFormData => {
    if (duplicateIncome) {
      return {
        description: `Copy of ${duplicateIncome.description}`,
        nextDue: duplicateIncome.nextDue,
        endDate: duplicateIncome.endDate || undefined,
        frequency: duplicateIncome.frequency,
        frequencyCount: duplicateIncome.frequencyCount,
        amount: duplicateIncome.amount,
        accountRowId: duplicateIncome.account.rowId,
      };
    }

    return {
      description: '',
      nextDue: todayIsoFormat(),
      endDate: undefined,
      frequency: Frequency.Months,
      frequencyCount: 1,
      amount: 0,
      accountRowId: '',
    };
  };

  const form = useForm<IncomeFormData>({
    resolver: zodResolver(incomeFormSchema),
    mode: 'onSubmit',
    defaultValues: getDefaultValues(),
  });

  const [error, setError] = useState<DisplayError | null>(null);

  const onSubmit = async (values: IncomeFormData) => {
    const income: CreateIncome = {
      ...values,
      endDate: values.endDate ?? null,
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

  const sheetTitle = duplicateIncome ? 'Duplicate Income' : 'Create Income';

  return (
    <IncomeSheet title={sheetTitle}>
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
        submitLabel="Create"
        accounts={accountsList}
      />
    </IncomeSheet>
  );
}

export default CreateIncomeForm;
