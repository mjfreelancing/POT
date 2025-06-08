import { useNavigate } from 'react-router';

import { useApiGetAllAccounts } from '@/api/hooks';
import LoadingMessage from '@/components/feedback/message/LoadingMessage';
import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';

import ExpenseSheet from '../../components/ExpenseSheet';
import CreateExpenseForm from '../components/CreateExpenseForm';

/**
 * Strategy for creating a completely new expense
 * Only needs to load accounts data
 */
function CreateNewExpense() {
  const navigate = useNavigate();
  const { data: accountsResult, isLoading: isAccountsLoading } =
    useApiGetAllAccounts();

  // Show loading state while accounts are loading
  if (isAccountsLoading) {
    return (
      <ExpenseSheet title="Create Expense">
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

  // Render form for new expense
  return <CreateExpenseForm accountsList={accountsResult.value} />;
}

export default CreateNewExpense;
