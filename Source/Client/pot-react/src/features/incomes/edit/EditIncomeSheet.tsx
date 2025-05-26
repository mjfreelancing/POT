import { zodResolver } from '@hookform/resolvers/zod';
import { useState, useRef } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router';

import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import { useApiGetIncomeById } from '@/api/incomes/hooks/useIncomes';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import { ErrorSheet } from '@/components/feedback/sheet/ErrorSheet';
import { Sheet, SheetContent } from '@/components/ui/sheet';
import type { EditIncome } from '@/data/incomes/income';
import type { DisplayError } from '@/lib/errors/displayError';
import { Frequency } from '@/lib/types';
import { localToday } from '@/lib/utils';

import { IncomeForm } from '../components/IncomeForm';
import { IncomeSheet } from '../components/IncomeSheet';
import { IncomeFormData, incomeFormSchema } from '../schemas/incomeFormSchema';
import { useEditIncome } from './hooks/useEditIncome';

const EditIncomeSheet = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: incomeResult, isLoading: isIncomeLoading } =
    useApiGetIncomeById(id!);
  const { data: accountsResult, isLoading: isAccountsLoading } =
    useApiGetAllAccounts();
  const { editIncome } = useEditIncome();

  const [error, setError] = useState<DisplayError | null>(null);

  const form = useForm<IncomeFormData>({
    resolver: zodResolver(incomeFormSchema),
    mode: 'onSubmit',
    defaultValues: {
      description: '',
      nextDue: localToday(),
      endDate: undefined,
      frequency: Frequency.Months,
      frequencyCount: 1,
      amount: 0,
      accountRowId: '',
    },
  });

  // Using useRef() to track if the form has been reset rather than useEffect() because there was a race condition where the
  // form reset would happen after the component had already rendered, causing the initial values to not be set correctly.
  const formHasReset = useRef(false);

  if (
    !isIncomeLoading &&
    !isAccountsLoading &&
    incomeResult.success &&
    accountsResult.success &&
    !formHasReset.current
  ) {
    const income = incomeResult.value;

    form.reset({
      description: income.description,
      nextDue: income.nextDue,
      endDate: income.endDate ?? undefined, // if endDate is null, set it to undefined to match the form's expected type
      frequency: income.frequency,
      frequencyCount: income.frequencyCount,
      amount: income.amount,
      accountRowId: income.account?.rowId,
    });

    formHasReset.current = true;
  }

  if (isIncomeLoading || isAccountsLoading || !formHasReset.current) {
    return (
      <IncomeSheet title="Edit Income">
        <LoadingMessage isLoading={true} />
      </IncomeSheet>
    );
  }

  const renderErrorSheet = (title: string, description: string) => (
    <Sheet open={true}>
      <SheetContent>
        <ErrorSheet
          title={title}
          description={description}
          onDismiss={() => navigate('/incomes')}
        />
      </SheetContent>
    </Sheet>
  );

  // Deal with failure to load income results
  if (incomeResult && !incomeResult.success) {
    return renderErrorSheet(
      incomeResult.error.code,
      incomeResult.error.description,
    );
  }

  // Deal with failure to load accounts
  if (accountsResult && !accountsResult.success) {
    return renderErrorSheet(
      accountsResult.error.code,
      accountsResult.error.description,
    );
  }

  const { rowId, eTag } = incomeResult.value;
  const accounts = accountsResult.value;

  const onSubmit = async (values: IncomeFormData) => {
    const payload: EditIncome = {
      rowId,
      eTag,
      ...values,
      endDate: values.endDate ?? null,
      accountRowId: values.accountRowId ?? null,
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
