import { AccrueExpenses } from '@/data';
import { FailResultBase, Result } from '@/lib';

import { usePost } from './useApi';

// Returning the mutation data as Result<void, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
const useApiAccrueExpenses = () => {
  const mutation = usePost<void, AccrueExpenses>('/accruals/accrue-expenses');

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
};

export { useApiAccrueExpenses };
