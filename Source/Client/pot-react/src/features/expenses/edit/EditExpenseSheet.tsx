import { zodResolver } from '@hookform/resolvers/zod';
import { useMemo } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router';

import { useApiGetAllAccounts, useApiGetExpenseById } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { useErrorContext } from '@/contexts';
import type { Account, EditExpense, Expense } from '@/data';

import ExpenseForm from '../components/ExpenseForm';
import ExpenseSheet from '../components/ExpenseSheet';
import type { ExpenseFormData } from '../schemas/expenseFormSchema';
import { expenseFormSchema } from '../schemas/expenseFormSchema';
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
      accrualStart: expenseData.accrualStart ?? undefined,
      accrualPolicy: expenseData.accrualPolicy,
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

  const { error, setError } = useErrorContext();

  const onSubmit = async (values: ExpenseFormData) => {
    const payload: EditExpense = {
      etag: expenseData.etag,
      ...values,
      accrualStart: values.accrualStart ?? null,
      endDate: values.endDate ?? null,
      accountRowId: values.accountRowId,
    };

    const result = await editExpense(expenseData.rowId, payload);

    if (result.success) {
      // Server responds with canonical AccrualStart, but this flow navigates away immediately,
      // so the next screen refresh reads canonical values from the API.
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
  const {
    data: expenseResult,
    isLoading: isExpenseLoading,
    isFetchedAfterMount: isExpenseFetchedAfterMount,
  } = useApiGetExpenseById(id!, { forceFresh: true });
  const { data: accountsResult, isLoading: isAccountsLoading } =
    useApiGetAllAccounts();

  // Show loading state before any data is available
  if (isExpenseLoading || isAccountsLoading || !isExpenseFetchedAfterMount) {
    return <LoadingMessage />;
  }

  // Handle API errors before mounting the sheet
  if (!expenseResult?.success) {
    return (
      <ErrorSheet
        title={expenseResult.error.code}
        description={expenseResult.error.description}
        onDismiss={() => navigate('/expenses')}
      />
    );
  }

  if (!accountsResult?.success) {
    return (
      <ErrorSheet
        title={accountsResult.error.code}
        description={accountsResult.error.description}
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
