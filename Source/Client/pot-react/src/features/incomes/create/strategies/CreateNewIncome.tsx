import { useNavigate } from 'react-router';

import { useApiGetAllAccounts } from '@/api/hooks';
import {
  ApiErrorSheetState,
  CreateSheetLoadingState,
} from '@/features/shared/sheets/asyncSheetStates';

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
      <CreateSheetLoadingState SheetShell={IncomeSheet} title="Create Income" />
    );
  }

  // Handle failure to load accounts
  if (!accountsResult || !accountsResult.success) {
    return (
      <ApiErrorSheetState
        result={accountsResult}
        fallbackTitle="Error Loading Accounts"
        fallbackDescription="Failed to load accounts. Please try again."
        onDismiss={() => navigate('/incomes')}
      />
    );
  }

  // Render form for new income
  return <CreateIncomeForm accountsList={accountsResult.value} />;
}

export default CreateNewIncome;
