import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router';

import { useErrorContext } from '@/contexts';
import type { CreateAccount } from '@/data';

import AccountForm from '../components/AccountForm';
import AccountSheet from '../components/AccountSheet';
import type { AccountFormData } from '../schemas/accountFormSchema';
import { accountFormSchema } from '../schemas/accountFormSchema';
import useCreateAccount from './hooks/useCreateAccount';

function CreateAccountSheet() {
  const { setError } = useErrorContext();
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
      <AccountForm
        form={form}
        onSubmit={onSubmit}
        onCancel={() => navigate('/accounts')}
        isEditMode={false}
        submitLabel="Create"
      />
    </AccountSheet>
  );
}

export default CreateAccountSheet;
