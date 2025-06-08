import { useNavigate } from 'react-router';

import { useApiGetAllAccounts, useApiGetExpenseById } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';

import ExpenseSheet from '../../components/ExpenseSheet';
import CreateExpenseForm from '../components/CreateExpenseForm';

type DuplicateExpenseProps = {
  duplicateId: string;
};

/**
 * Strategy for creating an expense by duplicating an existing one
 * Loads both accounts and the expense to duplicate in parallel
 */
function DuplicateExpense({ duplicateId }: DuplicateExpenseProps) {
  const navigate = useNavigate();

  // Load both accounts and duplicate expense in parallel
  const { data: accountsResult, isLoading: isAccountsLoading } =
    useApiGetAllAccounts();
  const { data: duplicateExpenseResult, isLoading: isDuplicateLoading } =
    useApiGetExpenseById(duplicateId);

  // Show loading state while either API call is in progress
  if (isAccountsLoading || isDuplicateLoading) {
    return (
      <ExpenseSheet title="Duplicate Expense">
        <LoadingMessage isLoading={true} />
      </ExpenseSheet>
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
        onDismiss={() => navigate('/expenses')}
      />
    );
  }

  // Handle error loading expense to duplicate
  if (!duplicateExpenseResult || !duplicateExpenseResult.success) {
    return (
      <ErrorSheet
        title={duplicateExpenseResult?.error?.code || 'Error Loading Expense'}
        description={
          duplicateExpenseResult?.error?.description ||
          'Failed to load the expense to duplicate. Please try again.'
        }
        onDismiss={() => navigate('/expenses')}
      />
    );
  }

  // Render form with duplicate data
  return (
    <CreateExpenseForm
      accountsList={accountsResult.value}
      duplicateExpense={duplicateExpenseResult.value}
    />
  );
}

export default DuplicateExpense;
