import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router';

import { useApiGetAccountById } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { EditAccount } from '@/data';
import { DisplayError } from '@/lib';

import AccountForm from '../components/AccountForm';
import AccountSheet from '../components/AccountSheet';
import {
  AccountFormData,
  accountFormSchema,
} from '../schemas/accountFormSchema';
import useEditAccount from './hooks/useEditAccount';

function EditAccountSheet() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data: accountResult, isLoading: isAccountLoading } =
    useApiGetAccountById(id!);
  const { editAccount } = useEditAccount();

  const [error, setError] = useState<DisplayError | null>(null);

  // Use placeholder defaults to prevent uncontrolled-to-controlled warnings.
  // Always reset with real data when it loads (see useEffect below).
  const form = useForm<AccountFormData>({
    resolver: zodResolver(accountFormSchema),
    mode: 'onSubmit',
    defaultValues: {
      bsb: '',
      number: '',
      description: '',
      balance: 0,
      reserved: 0,
    },
  });

  // React Hook Form only uses defaultValues on first mount. If incomeData changes (e.g. after import/cache refresh),
  // we must manually reset the form to reflect the latest data. Otherwise, the form will show stale values.
  useEffect(() => {
    if (accountResult?.success) {
      form.reset({
        bsb: accountResult.value.bsb,
        number: accountResult.value.number,
        description: accountResult.value.description,
        balance: accountResult.value.balance,
        reserved: accountResult.value.reserved,
      });
    }
  }, [accountResult, form]);

  if (isAccountLoading) {
    return (
      <AccountSheet title="Edit Account">
        <LoadingMessage isLoading={true} />
      </AccountSheet>
    );
  }

  // Deal with failure to load data
  if (accountResult && !accountResult.success) {
    return (
      <ErrorSheet
        title={accountResult.error.code}
        description={accountResult.error.description}
        onDismiss={() => navigate('/accounts')}
      />
    );
  }

  const { rowId, etag } = accountResult.value;

  const onSubmit = async (values: AccountFormData) => {
    const updatedAccount: EditAccount = {
      rowId,
      etag,
      ...values,
    };

    const editResult = await editAccount(updatedAccount);

    if (editResult.success) {
      navigate('/accounts');
    } else {
      setError({
        title: editResult.error.code,
        description: editResult.error.description,
      });
    }
  };

  return (
    <AccountSheet title="Edit Account">
      {error && (
        <ErrorSheet
          title={error.title}
          description={error.description}
          onDismiss={() => setError(null)}
        />
      )}
      <AccountForm
        form={form}
        onSubmit={onSubmit}
        onCancel={() => navigate('/accounts')}
        isEditMode={true}
        submitLabel="Save"
      />
    </AccountSheet>
  );
}

export default EditAccountSheet;
