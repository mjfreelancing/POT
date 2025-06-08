import { useSearchParams } from 'react-router';

import { CreateNewExpense, DuplicateExpense } from './strategies';

/**
 * Main component that chooses the appropriate strategy based on query parameters
 */
function CreateExpenseSheet() {
  const [searchParams] = useSearchParams();
  const duplicateId = searchParams.get('duplicate');

  if (duplicateId) {
    return <DuplicateExpense duplicateId={duplicateId} />;
  }

  return <CreateNewExpense />;
}

export default CreateExpenseSheet;
