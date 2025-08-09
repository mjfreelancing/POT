import { zodResolver } from '@hookform/resolvers/zod';
import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router';

import { useApiGetAllAccounts, useApiGetIncomeById } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import type { Account, EditIncome, Income } from '@/data';
import type { DisplayError } from '@/lib';

import IncomeForm from '../components/IncomeForm';
import IncomeSheet from '../components/IncomeSheet';
import { IncomeFormData, incomeFormSchema } from '../schemas/incomeFormSchema';
import useEditIncome from './hooks/useEditIncome';

type EditIncomeSheetInternalProps = {
  incomeData: Income;
  accountsList: Account[];
};

// This internal sheet is used to encapsulate the form logic and state management. We need
// to make sure the sheet is only rendered after both income and account data is loaded.
const EditIncomeSheetInternal: React.FC<EditIncomeSheetInternalProps> = ({
  incomeData,
  accountsList,
}) => {
  const navigate = useNavigate();
  const { editIncome } = useEditIncome();

  // Use placeholder defaults to prevent uncontrolled-to-controlled warnings.
  // Always reset with real data when it loads (see useEffect below).
  const form = useForm<IncomeFormData>({
    resolver: zodResolver(incomeFormSchema),
    mode: 'onSubmit',
    defaultValues: {
      excludeFromCalcs: false,
      description: '',
      nextDue: '',
      endDate: undefined,
      frequency: undefined,
      frequencyCount: 0,
      amount: 0,
      accountRowId: '',
      note: '',
    },
  });

  // React Hook Form only uses defaultValues on first mount. If incomeData changes (e.g. after import/cache refresh),
  // we must manually reset the form to reflect the latest data. Otherwise, the form will show stale values.
  useEffect(() => {
    form.reset({
      excludeFromCalcs: incomeData.excludeFromCalcs,
      description: incomeData.description,
      nextDue: incomeData.nextDue,
      endDate: incomeData.endDate ?? undefined,
      frequency: incomeData.frequency,
      frequencyCount: incomeData.frequencyCount,
      amount: incomeData.amount,
      accountRowId: incomeData.account.rowId,
      note: incomeData.note ?? '',
    });
  }, [incomeData, form]);

  const [error, setError] = useState<DisplayError | null>(null);

  const onSubmit = async (values: IncomeFormData) => {
    const payload: EditIncome = {
      rowId: incomeData.rowId,
      etag: incomeData.etag,
      ...values,
      endDate: values.endDate ?? null,
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
        submitLabel="Save"
        accounts={accountsList}
        isEditMode={true}
      />
    </IncomeSheet>
  );
};

function EditIncomeSheet() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: incomeResult, isLoading: isIncomeLoading } =
    useApiGetIncomeById(id!);
  const { data: accountsResult, isLoading: isAccountsLoading } =
    useApiGetAllAccounts();

  // Show loading state while either API call is in progress
  if (isIncomeLoading || isAccountsLoading) {
    return (
      <IncomeSheet title="Edit Income">
        <LoadingMessage isLoading={true} />
      </IncomeSheet>
    );
  }

  // Handle failure to load income
  if (!incomeResult || !incomeResult.success) {
    return (
      <ErrorSheet
        title={incomeResult?.error?.code || 'Error Loading Income'}
        description={
          incomeResult?.error?.description ||
          'Failed to load the income details. Please try again.'
        }
        onDismiss={() => navigate('/incomes')}
      />
    );
  }

  // Handle failure to load accounts
  if (!accountsResult || !accountsResult.success) {
    return (
      <ErrorSheet
        title={accountsResult?.error?.code || 'Error Loading Accounts'}
        description={
          accountsResult?.error?.description ||
          'Failed to load accounts. Please try again.'
        }
        onDismiss={() => navigate('/incomes')}
      />
    );
  }

  // If we reach here, both incomeResult and accountsResult are loaded and successful.
  // Their 'value' properties are guaranteed to be non-null.
  return (
    <EditIncomeSheetInternal
      incomeData={incomeResult.value}
      accountsList={accountsResult.value}
    />
  );
}

export default EditIncomeSheet;
