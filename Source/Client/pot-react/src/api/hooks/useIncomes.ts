import {
  CreateIncome,
  EditIncome,
  Identity,
  Income,
  PagedIncome,
} from '@/data';
import { FailResultBase } from '@/lib/result/failResultBase';
import { Result, SuccessResult } from '@/lib/result/result';

import { useDelete, useGet, usePost, usePut } from './useApi';

const useApiGetAllIncomes = () => {
  const query = useGet<PagedIncome>('/incomes', ['incomes']);
  const result = query.data as Result<PagedIncome, FailResultBase>;

  let data: Result<PagedIncome, FailResultBase>;

  // useGet() uses React Query which:
  // - returns undefined immediately and while loading
  // - returns the result when the query is successful
  if (result?.success) {
    // Not sorting client-side since the server performs this to ensure pagination is consistent.

    // // spreading [...result.value.results] to create a shallow copy of the array since sort()
    // // mutates the source array in the react-query cache.
    // const sortedResults = [...result.value.results].sort(compareIncomeNextDue);

    // const paged: PagedIncome = {
    //   ...result.value,
    //   results: sortedResults,
    // };

    const paged: PagedIncome = {
      ...result.value,
    };

    data = new SuccessResult(paged);
  } else {
    // type narrowed to FailResult<FailResultBase> since result cannot be undefined at this point
    data = result;
  }

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
  const mutation = usePut<Identity, EditIncome>('/incomes');

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

export {
  useApiCreateIncome,
  useApiDeleteIncome,
  useApiGetAllIncomes,
  useApiGetIncomeById,
  useApiUpdateIncome,
};
