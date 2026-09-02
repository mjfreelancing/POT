import { zodResolver } from '@hookform/resolvers/zod';
import { useMemo } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router';

import { useApiGetAccountById } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import { useErrorContext } from '@/contexts';
import type { Account, EditAccount } from '@/data';

import AccountForm from '../components/AccountForm';
import AccountSheet from '../components/AccountSheet';
import type { AccountFormData } from '../schemas/accountFormSchema';
import { accountFormSchema } from '../schemas/accountFormSchema';
import useEditAccount from './hooks/useEditAccount';

// This internal sheet is used to encapsulate the form logic and state management.
// We only render this once the account data is loaded.
type EditAccountSheetInternalProps = {
  accountData: Account;
};

const EditAccountSheetInternal: React.FC<EditAccountSheetInternalProps> = ({
  accountData,
}) => {
  const navigate = useNavigate();
  const { editAccount } = useEditAccount();
  const { setError } = useErrorContext();

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
      etag: accountData.etag,
      ...values,
    };

    const editResult = await editAccount(accountData.rowId, updatedAccount);

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
  const {
    data: accountResult,
    isLoading: isAccountLoading,
    isFetchedAfterMount: isAccountFetchedAfterMount,
  } = useApiGetAccountById(id!, { forceFresh: true });

  // Show loading state before any data is available
  if (isAccountLoading || !isAccountFetchedAfterMount) {
    return <LoadingMessage />;
  }

  // Handle API errors before mounting the sheet
  if (!accountResult?.success) {
    return (
      <ErrorSheet
        title={accountResult.error.code}
        description={accountResult.error.description}
        onDismiss={() => navigate('/accounts')}
      />
    );
  }

  // Only mount the sheet once we have the data
  return <EditAccountSheetInternal accountData={accountResult.value} />;
}

export default EditAccountSheet;
