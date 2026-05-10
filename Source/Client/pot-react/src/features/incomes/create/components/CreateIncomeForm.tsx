import { zodResolver } from '@hookform/resolvers/zod';
import { useCallback, useEffect, useMemo } from 'react';
import { useForm } from 'react-hook-form';
import { useLocation, useNavigate } from 'react-router';

import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { useErrorContext } from '@/contexts';
import type { Account, CreateIncome, Income } from '@/data';
import { Frequency, todayIsoFormat } from '@/lib';

import IncomeForm from '../../components/IncomeForm';
import IncomeSheet from '../../components/IncomeSheet';
import type { IncomeFormData } from '../../schemas/incomeFormSchema';
import { incomeFormSchema } from '../../schemas/incomeFormSchema';
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
  const location = useLocation();
  const { createIncome } = useCreateIncome();
  const returnPath = `/incomes${location.search}`;

  const getDefaultValues = useCallback((): IncomeFormData => {
    if (duplicateIncome) {
      return {
        description: `Copy of ${duplicateIncome.description}`,
        nextDue: duplicateIncome.nextDue,
        endDate: duplicateIncome.endDate || undefined,
        frequency: duplicateIncome.frequency,
        frequencyCount: duplicateIncome.frequencyCount,
        amount: duplicateIncome.amount,
        accountRowId: duplicateIncome.account.rowId,
        note: duplicateIncome.note ?? '',
        excludeFromCalcs: duplicateIncome.excludeFromCalcs,
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
      note: '',
      excludeFromCalcs: false,
    };
  }, [duplicateIncome]);

  // Memoize default values to track when duplicateIncome changes - this fixes an issue where
  // a user edits an item, then navigates to duplicate it, and the form shows stale data.
  const defaultValues = useMemo(() => getDefaultValues(), [getDefaultValues]);

  const form = useForm<IncomeFormData>({
    resolver: zodResolver(incomeFormSchema),
    mode: 'onSubmit',
    defaultValues,
  });

  // Reset form when duplicateIncome changes (e.g., when fresh data arrives from API)
  useEffect(() => {
    form.reset(defaultValues);
  }, [defaultValues, form]);

  const { error, setError } = useErrorContext();

  const onSubmit = async (values: IncomeFormData) => {
    const income: CreateIncome = {
      ...values,
      endDate: values.endDate ?? null,
    };

    const result = await createIncome(income);

    if (result.success) {
      navigate(returnPath, { replace: true });
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
        onCancel={() => navigate(returnPath, { replace: true })}
        submitLabel="Create"
        accounts={accountsList}
      />
    </IncomeSheet>
  );
}

export default CreateIncomeForm;
