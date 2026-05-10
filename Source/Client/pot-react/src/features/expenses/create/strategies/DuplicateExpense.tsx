import { useLocation, useNavigate } from 'react-router';

import { useApiGetAllAccounts, useApiGetExpenseById } from '@/api/hooks';
import {
  ApiErrorSheetState,
  CreateSheetLoadingState,
} from '@/features/shared/sheets/asyncSheetStates';

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
  const location = useLocation();
  const returnPath = `/expenses${location.search}`;

  // Load both accounts and duplicate expense in parallel
  const { data: accountsResult, isLoading: isAccountsLoading } =
    useApiGetAllAccounts();
  const { data: duplicateExpenseResult, isLoading: isDuplicateLoading } =
    useApiGetExpenseById(duplicateId);

  // Show loading state while either API call is in progress
  if (isAccountsLoading || isDuplicateLoading) {
    return (
      <CreateSheetLoadingState
        SheetShell={ExpenseSheet}
        title="Duplicate Expense"
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

  // Handle error loading expense to duplicate
  if (!duplicateExpenseResult || !duplicateExpenseResult.success) {
    return (
      <ApiErrorSheetState
        result={duplicateExpenseResult}
        fallbackTitle="Error Loading Expense"
        fallbackDescription="Failed to load the expense to duplicate. Please try again."
        onDismiss={() => navigate(returnPath, { replace: true })}
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
