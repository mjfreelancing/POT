import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router';

import { useApiGetAccountById } from '@/api/accounts/hooks/useAccounts';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { EditAccount } from '@/data/account';
import { DisplayError } from '@/lib/errors/displayError';

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

  // Initialize form with default values to prevent uncontrolled-to-controlled input warnings
  // while LoadingMessage is displayed. When the account data loads, form.reset() will
  // update these values with the actual account details.
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
      bsb: values.bsb,
      number: values.number,
      description: values.description,
      balance: values.balance,
      reserved: values.reserved,
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
        readOnlyIdentifiers={true}
        submitLabel="Save"
      />
    </AccountSheet>
  );
}

export default EditAccountSheet;
