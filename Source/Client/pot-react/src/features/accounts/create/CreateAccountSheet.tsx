import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router';

import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { CreateAccount } from '@/data';
import { DisplayError } from '@/lib/errors/displayError';

import AccountForm from '../components/AccountForm';
import AccountSheet from '../components/AccountSheet';
import {
  AccountFormData,
  accountFormSchema,
} from '../schemas/accountFormSchema';
import useCreateAccount from './hooks/useCreateAccount';

function CreateAccountSheet() {
  const [error, setError] = useState<DisplayError | null>(null);
  const navigate = useNavigate();
  const { createAccount } = useCreateAccount();

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

  const onSubmit = async (values: AccountFormData) => {
    const account: CreateAccount = values;
    const result = await createAccount(account);

    if (result.success) {
      navigate('/accounts');
    } else {
      setError({
        title: result.error.code,
        description: result.error.description,
      });
    }
  };

  return (
    <AccountSheet title="Create Account">
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
        readOnlyIdentifiers={false}
        submitLabel="Create"
      />
    </AccountSheet>
  );
}

export default CreateAccountSheet;
