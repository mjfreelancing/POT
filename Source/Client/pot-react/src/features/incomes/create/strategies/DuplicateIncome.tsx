import { useLocation, useNavigate } from 'react-router';

import { useApiGetAllAccounts, useApiGetIncomeById } from '@/api/hooks';
import {
  ApiErrorSheetState,
  CreateSheetLoadingState,
} from '@/features/shared/sheets/asyncSheetStates';

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
  const location = useLocation();
  const returnPath = `/incomes${location.search}`;

  // Load both accounts and duplicate income in parallel
  const { data: accountsResult, isLoading: isAccountsLoading } =
    useApiGetAllAccounts();
  const { data: duplicateIncomeResult, isLoading: isDuplicateLoading } =
    useApiGetIncomeById(duplicateId);

  // Show loading state while either API call is in progress
  if (isAccountsLoading || isDuplicateLoading) {
    return (
      <CreateSheetLoadingState
        SheetShell={IncomeSheet}
        title="Duplicate Income"
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

  // Handle error loading income to duplicate
  if (!duplicateIncomeResult || !duplicateIncomeResult.success) {
    return (
      <ApiErrorSheetState
        result={duplicateIncomeResult}
        fallbackTitle="Error Loading Income"
        fallbackDescription="Failed to load the income to duplicate. Please try again."
        onDismiss={() => navigate(returnPath, { replace: true })}
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
