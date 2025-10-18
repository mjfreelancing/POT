import { useMemo } from 'react';

import type {
  CreateIncome,
  EditIncome,
  Identity,
  Income,
  RenewIncomes,
  ToggleExcludeIncomes,
} from '@/data';
import { compareIncomeNextDue } from '@/data';
import type { FailResultBase, Result } from '@/lib';
import { SuccessResult } from '@/lib';

import { useDelete, useGet, usePost, usePutWithId } from './useApi';

const useApiGetAllIncomes = () => {
  const query = useGet<Income[]>('/incomes', ['incomes']);
  const result = query.data as Result<Income[], FailResultBase>;

  const data = useMemo(() => {
    if (result?.success) {
      // spreading [...result.value] to create a shallow copy of the array since
      // sort() mutates the source array in the react-query cache.
      const sortedResults = [...result.value].sort(compareIncomeNextDue);
      return new SuccessResult(sortedResults);
    }

    // type narrowed to FailResult<FailResultBase> since result cannot be undefined at this point
    return result;
  }, [result]);

  // Returns isLoading, isError, error, and data (and anything else from React Query)
  return { ...query, data };
};

const useApiGetIncomeById = (id: string) => {
  const query = useGet<Income>(`/incomes/${id}`, ['incomes', id]);

  // Cast to Result<Income, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
  // This allows TypeScript to infer that when !success, error must exist, and when success, value must exist.
  // Without the cast, TypeScript can't determine the relationship between success and error/value properties.
  return {
    ...query,
    data: query.data as Result<Income, FailResultBase>,
  };
};

const useApiCreateIncome = () => {
  const mutation = usePost<Identity, CreateIncome>('/incomes');

  // Returning the mutation data as Result<Identity, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
};

// Returning the mutation data as Result<Identity, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
const useApiUpdateIncome = () => {
  const mutation = usePutWithId<Identity, EditIncome>(
    (id: string) => `/incomes/${id}`,
  );
  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
};

// Returning the mutation data as Result<void, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
const useApiDeleteIncome = (id: string) => {
  const mutation = useDelete<void>(`/incomes/${id}`);

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
};

const useApiToggleExcludeIncomes = () => {
  const mutation = usePost<void, ToggleExcludeIncomes>(
    '/incomes/toggleExclude',
  );

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
};

// Returning the mutation data as Result<void, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
const useApiRenewIncomes = () => {
  const mutation = usePost<void, RenewIncomes>('/incomes/renew');

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
};

export {
  useApiCreateIncome,
  useApiDeleteIncome,
  useApiGetAllIncomes,
  useApiGetIncomeById,
  useApiRenewIncomes,
  useApiToggleExcludeIncomes,
  useApiUpdateIncome,
};
