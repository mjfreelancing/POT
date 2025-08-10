import { zodResolver } from '@hookform/resolvers/zod';
import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router';

import { useApiGetAllAccounts, useApiGetExpenseById } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import type { Account, EditExpense, Expense } from '@/data';
import type { DisplayError } from '@/lib';

import ExpenseForm from '../components/ExpenseForm';
import ExpenseSheet from '../components/ExpenseSheet';
import {
  ExpenseFormData,
  expenseFormSchema,
} from '../schemas/expenseFormSchema';
import useEditExpense from './hooks/useEditExpense';

type EditExpenseSheetInternalProps = {
  expenseData: Expense;
  accountsList: Account[];
};

// This internal sheet is used to encapsulate the form logic and state management. We need
// to make sure the sheet is only rendered after both expense and account data is loaded.
const EditExpenseSheetInternal: React.FC<EditExpenseSheetInternalProps> = ({
  expenseData,
  accountsList,
}) => {
  const navigate = useNavigate();
  const { editExpense } = useEditExpense();

  const defaultValues = useMemo(
    () => ({
      excludeFromCalcs: expenseData.excludeFromCalcs,
      description: expenseData.description,
      nextDue: expenseData.nextDue,
      accrualStart: expenseData.accrualStart,
      endDate: expenseData.endDate ?? undefined,
      frequency: expenseData.frequency,
      frequencyCount: expenseData.frequencyCount,
      amount: expenseData.amount,
      note: expenseData.note ?? '',
      accountRowId: expenseData.account.rowId,
    }),
    [expenseData],
  );

  const form = useForm<ExpenseFormData>({
    resolver: zodResolver(expenseFormSchema),
    mode: 'onSubmit',
    defaultValues,
    values: defaultValues, // Use current values to avoid initial blank form
  });

  const [error, setError] = useState<DisplayError | null>(null);

  const onSubmit = async (values: ExpenseFormData) => {
    const payload: EditExpense = {
      rowId: expenseData.rowId,
      etag: expenseData.etag,
      ...values,
      endDate: values.endDate ?? null,
      accountRowId: values.accountRowId,
    };

    const result = await editExpense(payload);

    if (result.success) {
      navigate('/expenses');
    } else {
      setError({
        title: result.error.code,
        description: result.error.description,
      });
    }
  };

  return (
    <ExpenseSheet title="Edit Expense">
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
        submitLabel="Save"
        accounts={accountsList}
        isEditMode={true}
      />
    </ExpenseSheet>
  );
};

function EditExpenseSheet() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: expenseResult, isLoading: isExpenseLoading } =
    useApiGetExpenseById(id!);
  const { data: accountsResult, isLoading: isAccountsLoading } =
    useApiGetAllAccounts();

  // Show loading state before any data is available
  if (isExpenseLoading || isAccountsLoading) {
    return <LoadingMessage />;
  }

  // Handle API errors before mounting the sheet
  if (!expenseResult?.success) {
    return (
      <ErrorSheet
        title={expenseResult?.error?.code ?? 'Error Loading Expense'}
        description={
          expenseResult?.error?.description ??
          'Failed to load the expense details. Please try again.'
        }
        onDismiss={() => navigate('/expenses')}
      />
    );
  }

  if (!accountsResult?.success) {
    return (
      <ErrorSheet
        title={accountsResult?.error?.code ?? 'Error Loading Accounts'}
        description={
          accountsResult?.error?.description ??
          'Failed to load accounts. Please try again.'
        }
        onDismiss={() => navigate('/expenses')}
      />
    );
  }

  // Only mount the sheet once we have all the data
  return (
    <EditExpenseSheetInternal
      expenseData={expenseResult.value}
      accountsList={accountsResult.value}
    />
  );
}

export default EditExpenseSheet;
