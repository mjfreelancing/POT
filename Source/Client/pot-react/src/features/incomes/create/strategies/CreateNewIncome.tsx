import { useNavigate } from 'react-router';

import { useApiGetAllAccounts } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';

import IncomeSheet from '../../components/IncomeSheet';
import CreateIncomeForm from '../components/CreateIncomeForm';

/**
 * Strategy for creating a completely new income
 * Only needs to load accounts data
 */
function CreateNewIncome() {
  const navigate = useNavigate();
  const { data: accountsResult, isLoading: isAccountsLoading } =
    useApiGetAllAccounts();

  // Show loading state while accounts are loading
  if (isAccountsLoading) {
    return (
      <IncomeSheet title="Create Income">
        <LoadingMessage isLoading={true} />
      </IncomeSheet>
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

  // Render form for new income
  return (
    <CreateIncomeForm
      accountsList={accountsResult.value}
      duplicateIncome={undefined}
    />
  );
}

export default CreateNewIncome;
