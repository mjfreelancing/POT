import { zodResolver } from '@hookform/resolvers/zod';
import { useCallback, useEffect, useMemo } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router';

import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { useErrorContext } from '@/contexts';
import type { Account, CreateExpense, Expense } from '@/data';
import { Frequency, todayIsoFormat } from '@/lib';

import ExpenseForm from '../../components/ExpenseForm';
import ExpenseSheet from '../../components/ExpenseSheet';
import type { ExpenseFormData } from '../../schemas/expenseFormSchema';
import { expenseFormSchema } from '../../schemas/expenseFormSchema';
import useCreateExpense from '../hooks/useCreateExpense';

type CreateExpenseFormProps = {
  accountsList: Account[];
  duplicateExpense?: Expense;
};

/**
 * Pure form component that handles expense creation logic
 * Receives all required data as props - no API calls or loading states
 */
function CreateExpenseForm({
  accountsList,
  duplicateExpense,
}: CreateExpenseFormProps) {
  const navigate = useNavigate();
  const { createExpense } = useCreateExpense();

  const getDefaultValues = useCallback((): ExpenseFormData => {
    if (duplicateExpense) {
      return {
        description: `Copy of ${duplicateExpense.description}`,
        nextDue: duplicateExpense.nextDue,
        accrualStart: duplicateExpense.accrualStart,
        endDate: duplicateExpense.endDate || undefined,
        frequency: duplicateExpense.frequency,
        frequencyCount: duplicateExpense.frequencyCount,
        amount: duplicateExpense.amount,
        accountRowId: duplicateExpense.account.rowId,
        note: duplicateExpense.note ?? '',
        excludeFromCalcs: duplicateExpense.excludeFromCalcs,
      };
    }

    return {
      description: '',
      nextDue: todayIsoFormat(),
      accrualStart: todayIsoFormat(),
      endDate: undefined,
      frequency: Frequency.Months,
      frequencyCount: 1,
      amount: 0,
      accountRowId: '',
      note: '',
      excludeFromCalcs: false,
    };
  }, [duplicateExpense]);

  // Memoize default values to track when duplicateExpense changes - this fixes an issue where
  // a user edits an item, then navigates to duplicate it, and the form shows stale data.
  const defaultValues = useMemo(() => getDefaultValues(), [getDefaultValues]);

  const form = useForm<ExpenseFormData>({
    resolver: zodResolver(expenseFormSchema),
    mode: 'onSubmit',
    defaultValues,
  });

  // Reset form when duplicateExpense changes (e.g., when fresh data arrives from API)
  useEffect(() => {
    form.reset(defaultValues);
  }, [defaultValues, form]);

  const { error, setError } = useErrorContext();

  const onSubmit = async (values: ExpenseFormData) => {
    const expense: CreateExpense = {
      ...values,
      endDate: values.endDate ?? null,
    };

    const result = await createExpense(expense);

    if (result.success) {
      navigate('/expenses');
    } else {
      setError({
        title: result.error.code,
        description: result.error.description,
      });
    }
  };

  const sheetTitle = duplicateExpense ? 'Duplicate Expense' : 'Create Expense';

  return (
    <ExpenseSheet title={sheetTitle}>
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
      <ExpenseForm
        form={form}
        onSubmit={onSubmit}
        onCancel={() => navigate('/expenses')}
        submitLabel="Create"
        accounts={accountsList}
      />
    </ExpenseSheet>
  );
}

export default CreateExpenseForm;
