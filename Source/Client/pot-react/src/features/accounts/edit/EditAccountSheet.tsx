import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router';

import { useApiGetAccountById } from '@/api/accounts/hooks/useAccounts';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import { ErrorSheet } from '@/components/feedback/sheet/ErrorSheet';
import { Sheet, SheetContent } from '@/components/ui/sheet';
import { EditAccount } from '@/data/accounts/account';
import { DisplayError } from '@/lib/errors/displayError';

import { AccountForm } from '../components/AccountForm';
import { AccountSheet } from '../components/AccountSheet';
import { AccountFormSchema, accountFormSchema } from '../schemas/accountSchema';
import { useEditAccount } from './hooks/useEditAccount';

const EditAccountSheet = () => {
  const [error, setError] = useState<DisplayError | null>(null);
  const navigate = useNavigate();
  const { id } = useParams();
  const { data: result, isLoading } = useApiGetAccountById(id!);
  const { editAccount } = useEditAccount();

  // Initialize form with default values to prevent uncontrolled-to-controlled input warnings
  // while LoadingMessage is displayed. When the account data loads, form.reset() will
  // update these values with the actual account details.
  const form = useForm<AccountFormSchema>({
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
    if (result?.success) {
      form.reset({
        bsb: result.value.bsb,
        number: result.value.number,
        description: result.value.description,
        balance: result.value.balance,
        reserved: result.value.reserved,
      });
    }
  }, [form, result]);

  if (isLoading) {
    return (
      <AccountSheet title="Edit Account">
        <LoadingMessage isLoading={true} />
      </AccountSheet>
    );
  }

  {
    /* Deal with failure to load account data */
  }
  if (result && !result.success) {
    return (
      <Sheet open={true}>
        <SheetContent>
          <ErrorSheet
            title={result.error.code}
            description={result.error.description}
            onDismiss={() => navigate('/accounts')}
          />
        </SheetContent>
      </Sheet>
    );
  }

  const { rowId, eTag } = result.value;

  const onSubmit = async (values: AccountFormSchema) => {
    const updatedAccount: EditAccount = {
      rowId,
      eTag,
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
        readOnlyFields={true}
        submitLabel="Save"
      />
    </AccountSheet>
  );
};

export default EditAccountSheet;
