import {
  CreateExpense,
  EditExpense,
  Expense,
  Identity,
  PagedExpense,
} from '@/data';
import { FailResultBase } from '@/lib/result/failResultBase';
import { Result, SuccessResult } from '@/lib/result/result';

import { useDelete, useGet, usePost, usePut } from './useApi';

const useApiGetAllExpenses = () => {
  const query = useGet<PagedExpense>('/expenses', ['expenses']);
  const result = query.data as Result<PagedExpense, FailResultBase>;

  let data: Result<PagedExpense, FailResultBase>;

  // useGet() uses React Query which:
  // - returns undefined immediately and while loading
  // - returns the result when the query is successful
  if (result?.success) {
    // Not sorting client-side since the server performs this to ensure pagination is consistent.

    // // spreading [...result.value.results] to create a shallow copy of the array since sort()
    // // mutates the source array in the react-query cache.
    // const sortedResults = [...result.value.results].sort(compareExpenseNextDue);

    // const paged: PagedExpense = {
    //   ...result.value,
    //   results: sortedResults,
    // };

    const paged: PagedExpense = {
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

const useApiGetExpenseById = (id: string) => {
  const query = useGet<Expense>(`/expenses/${id}`, ['expenses', id]);

  // Cast to Result<Expense, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
  // This allows TypeScript to infer that when !success, error must exist, and when success, value must exist.
  // Without the cast, TypeScript can't determine the relationship between success and error/value properties.
  return {
    ...query,
    data: query.data as Result<Expense, FailResultBase>,
  };
};

const useApiCreateExpense = () => {
  const mutation = usePost<Identity, CreateExpense>('/expenses');

  // Returning the mutation data as Result<Identity, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
};

// Returning the mutation data as Result<Identity, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
const useApiUpdateExpense = () => {
  const mutation = usePut<Identity, EditExpense>('/expenses');

  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
};

// Returning the mutation data as Result<void, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
const useApiDeleteExpense = (id: string) => {
  const mutation = useDelete<void>(`/expenses/${id}`);

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
};

export {
  useApiCreateExpense,
  useApiDeleteExpense,
  useApiGetAllExpenses,
  useApiGetExpenseById,
  useApiUpdateExpense,
};
