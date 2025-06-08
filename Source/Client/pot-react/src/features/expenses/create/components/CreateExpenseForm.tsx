import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router';

import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import type { Account, CreateExpense, Expense } from '@/data';
import { DisplayError } from '@/lib/errors/displayError';
import { Frequency } from '@/lib/types';
import { localToday } from '@/lib/utils';

import ExpenseForm from '../../components/ExpenseForm';
import ExpenseSheet from '../../components/ExpenseSheet';
import {
  ExpenseFormData,
  expenseFormSchema,
} from '../../schemas/expenseFormSchema';
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

  const getDefaultValues = (): ExpenseFormData => {
    if (duplicateExpense) {
      return {
        description: `Copy of ${duplicateExpense.description}`,
        nextDue: duplicateExpense.nextDue,
        endDate: duplicateExpense.endDate || undefined,
        frequency: duplicateExpense.frequency,
        frequencyCount: duplicateExpense.frequencyCount,
        amount: duplicateExpense.amount,
        accountRowId: duplicateExpense.account.rowId,
      };
    }

    return {
      description: '',
      nextDue: localToday(),
      endDate: undefined,
      frequency: Frequency.Months,
      frequencyCount: 1,
      amount: 0,
      accountRowId: '',
    };
  };

  const form = useForm<ExpenseFormData>({
    resolver: zodResolver(expenseFormSchema),
    mode: 'onSubmit',
    defaultValues: getDefaultValues(),
  });

  const [error, setError] = useState<DisplayError | null>(null);

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
        readOnlyIdentifiers={false}
        submitLabel="Create"
        accounts={accountsList}
      />
    </ExpenseSheet>
  );
}

export default CreateExpenseForm;
