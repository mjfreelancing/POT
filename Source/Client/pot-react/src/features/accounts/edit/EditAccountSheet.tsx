import { zodResolver } from '@hookform/resolvers/zod';
import { useMemo, useState } from 'react';
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

// This internal sheet is used to encapsulate the form logic and state management.
// We only render this once the account data is loaded.
type EditAccountSheetInternalProps = {
  accountData: EditAccount;
};

const EditAccountSheetInternal: React.FC<EditAccountSheetInternalProps> = ({
  accountData,
}) => {
  const navigate = useNavigate();
  const { editAccount } = useEditAccount();
  const [error, setError] = useState<DisplayError | null>(null);

  const defaultValues = useMemo(
    () => ({
      bsb: accountData.bsb,
      number: accountData.number,
      description: accountData.description,
      balance: accountData.balance,
      reserved: accountData.reserved,
    }),
    [accountData],
  );

  const form = useForm<AccountFormData>({
    resolver: zodResolver(accountFormSchema),
    mode: 'onSubmit',
    defaultValues,
    values: defaultValues, // Use current values to avoid initial blank form
  });

  const onSubmit = async (values: AccountFormData) => {
    const updatedAccount: EditAccount = {
      rowId: accountData.rowId,
      etag: accountData.etag,
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
};

function EditAccountSheet() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data: accountResult, isLoading: isAccountLoading } =
    useApiGetAccountById(id!);

  // Show loading state before any data is available
  if (isAccountLoading) {
    return <LoadingMessage />;
  }

  // Handle API errors before mounting the sheet
  if (!accountResult?.success) {
    return (
      <ErrorSheet
        title={accountResult?.error?.code ?? 'Error Loading Account'}
        description={
          accountResult?.error?.description ??
          'Failed to load the account details. Please try again.'
        }
        onDismiss={() => navigate('/accounts')}
      />
    );
  }

  // Only mount the sheet once we have the data
  return <EditAccountSheetInternal accountData={accountResult.value} />;
}

export default EditAccountSheet;
