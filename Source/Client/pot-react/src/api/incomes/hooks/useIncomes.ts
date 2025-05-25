import {
  compareIncomeNextDue,
  CreateIncome,
  Income,
  PagedIncome,
} from '@/data/incomes/income';
import { FailResultBase } from '@/lib/result/failResultBase';
import { Result, SuccessResult } from '@/lib/result/result';

import { useGet, usePost } from '../../hooks/useApi';
import { Identity } from '@/data/identity';

const useApiGetAllIncomes = () => {
  const query = useGet<PagedIncome>('/incomes', ['incomes']);
  const result = query.data as Result<PagedIncome, FailResultBase>;

  let data: Result<PagedIncome, FailResultBase>;

  // useGet() uses React Query which:
  // - returns undefined immediately and while loading
  // - returns the result when the query is successful
  if (result?.success) {
    // spreading [...result.value.results] to create a shallow copy of the array since sort()
    // mutates the source array in the react-query cache.
    const sortedResults = [...result.value.results].sort(compareIncomeNextDue);

    const paged: PagedIncome = {
      ...result.value,
      results: sortedResults,
    };

    data = new SuccessResult(paged);
  } else {
    // type narrowed to FailResult<FailResultBase> since result cannot be undefined at this point
    data = result;
  }

  // Returns isLoading, isError, error, and data (and anything else from React Query)
  return { ...query, data };
};

// const useApiGetAccountById = (id: string) => {
//   const query = useGet<Account>(`/accounts/${id}`, ['accounts', id]);

//   // Cast to Result<Account, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
//   // This allows TypeScript to infer that when !success, error must exist, and when success, value must exist.
//   // Without the cast, TypeScript can't determine the relationship between success and error/value properties.
//   return {
//     ...query,
//     data: query.data as Result<Account, FailResultBase>,
//   };
// };

const useApiCreateIncome = () => {
  const mutation = usePost<Identity, CreateIncome>('/incomes');

  // Returning the mutation data as Result<Identity, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
};

// // Returning the mutation data as Result<Identity, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
// const useApiUpdateAccount = () => {
//   const mutation = usePut<Identity, EditAccount>('/accounts');

//   return {
//     ...mutation,
//     data: mutation.data as Result<Identity, FailResultBase>,
//   };
// };

// // Not performing type narrowing as this method currently returns a mutation without Result<T> handling
// const useApiDeleteAccount = (id: string) => {
//   return useDelete<void>(`/accounts/${id}`);
// };

export {
  useApiCreateIncome,
  // useApiDeleteAccount,
  // useApiGetAccountById,
  useApiGetAllIncomes,
  //useApiUpdateAccount,
};
