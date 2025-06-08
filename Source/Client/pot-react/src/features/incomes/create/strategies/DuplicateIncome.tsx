import { useNavigate } from 'react-router';

import { useApiGetAllAccounts } from '@/api/accounts/hooks/useAccounts';
import { useApiGetIncomeById } from '@/api/incomes/hooks/useIncomes';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';

import IncomeSheet from '../../components/IncomeSheet';
import CreateIncomeForm from '../components/CreateIncomeForm';

type DuplicateIncomeProps = {
  duplicateId: string;
};

/**
 * Strategy for creating an income by duplicating an existing one
 * Loads both accounts and the income to duplicate in parallel
 */
function DuplicateIncome({ duplicateId }: DuplicateIncomeProps) {
  const navigate = useNavigate();

  // Load both accounts and duplicate income in parallel
  const { data: accountsResult, isLoading: isAccountsLoading } =
    useApiGetAllAccounts();
  const { data: duplicateIncomeResult, isLoading: isDuplicateLoading } =
    useApiGetIncomeById(duplicateId);

  // Show loading state while either API call is in progress
  if (isAccountsLoading || isDuplicateLoading) {
    return (
      <IncomeSheet title="Duplicate Income">
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

  // Handle error loading income to duplicate
  if (!duplicateIncomeResult || !duplicateIncomeResult.success) {
    return (
      <ErrorSheet
        title={duplicateIncomeResult?.error?.code || 'Error Loading Income'}
        description={
          duplicateIncomeResult?.error?.description ||
          'Failed to load the income to duplicate. Please try again.'
        }
        onDismiss={() => navigate('/incomes')}
      />
    );
  }

  // Render form with duplicate data
  return (
    <CreateIncomeForm
      accountsList={accountsResult.value}
      duplicateIncome={duplicateIncomeResult.value}
    />
  );
}

export default DuplicateIncome;
