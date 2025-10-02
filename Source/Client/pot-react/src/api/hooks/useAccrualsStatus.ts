import {
  AccrualsStatus,
  AccrualsStatusInput,
  AccrueAccountExpensesInput,
} from '@/data';
import { FailResultBase, Result } from '@/lib';

import { useGet, usePost } from './useApi';

// Returning the mutation data as Result<void, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
const useApiAccrueAccountExpenses = () => {
  const mutation = usePost<void, AccrueAccountExpensesInput>(
    '/accruals/accrue-expenses',
  );

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
};

const useApiAccrualsStatus = (input: AccrualsStatusInput) => {
  const hasAccounts = input.accountRowIds.length > 0;

  const queryString = hasAccounts
    ? `accountRowIds=${input.accountRowIds.join(',')}`
    : undefined;

  // Always querying the status since the collection of account id's is rarely going to change.
  // Even though the cache will expire, it's best to ensure the dashboard is reflective of the actual status.
  const query = useGet<Result<AccrualsStatus, FailResultBase>>(
    `/accruals/status?${queryString}`,
    ['accruals', ...input.accountRowIds],
    { enabled: hasAccounts, staleTime: 0, gcTime: 0 },
  );

  return {
    ...query,
    data: query.data as Result<AccrualsStatus, FailResultBase>,
  };
};

export { useApiAccrualsStatus, useApiAccrueAccountExpenses };
