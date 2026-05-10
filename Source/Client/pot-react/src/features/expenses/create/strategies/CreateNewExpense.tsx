import { useLocation, useNavigate } from 'react-router';

import { useApiGetAllAccounts } from '@/api/hooks';
import {
  ApiErrorSheetState,
  CreateSheetLoadingState,
} from '@/features/shared/sheets/asyncSheetStates';

import ExpenseSheet from '../../components/ExpenseSheet';
import CreateExpenseForm from '../components/CreateExpenseForm';

/**
 * Strategy for creating a completely new expense
 * Only needs to load accounts data
 */
function CreateNewExpense() {
  const navigate = useNavigate();
  const location = useLocation();
  const returnPath = `/expenses${location.search}`;
  const { data: accountsResult, isLoading: isAccountsLoading } =
    useApiGetAllAccounts();

  // Show loading state while accounts are loading
  if (isAccountsLoading) {
    return (
      <CreateSheetLoadingState
        SheetShell={ExpenseSheet}
        title="Create Expense"
      />
    );
  }

  // Handle failure to load accounts
  if (!accountsResult || !accountsResult.success) {
    return (
      <ApiErrorSheetState
        result={accountsResult}
        fallbackTitle="Error Loading Accounts"
        fallbackDescription="Failed to load accounts. Please try again."
        onDismiss={() => navigate(returnPath, { replace: true })}
      />
    );
  }

  // Render form for new expense
  return <CreateExpenseForm accountsList={accountsResult.value} />;
}

export default CreateNewExpense;
