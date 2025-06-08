import { useSearchParams } from 'react-router';

import { CreateNewIncome, DuplicateIncome } from './strategies';

/**
 * Main component that chooses the appropriate strategy based on query parameters
 */
function CreateIncomeSheet() {
  const [searchParams] = useSearchParams();
  const duplicateId = searchParams.get('duplicate');

  if (duplicateId) {
    return <DuplicateIncome duplicateId={duplicateId} />;
  }

  return <CreateNewIncome />;
}

export default CreateIncomeSheet;
