import { useMemo } from 'react';

import type {
  CreateExpense,
  EditExpense,
  Expense,
  Identity,
  PagedExpense,
  RenewExpenses,
  ToggleExcludeExpenses,
} from '@/data';
import type { FailResultBase, Result } from '@/lib';
import { SuccessResult } from '@/lib';

import { useDelete, useGet, usePost, usePutWithId } from './useApi';

type ByIdOptions = {
  forceFresh?: boolean;
};

const useApiGetAllExpenses = () => {
  const query = useGet<PagedExpense>('/expenses', ['expenses']);
  const result = query.data as Result<PagedExpense, FailResultBase>;

  const data = useMemo(() => {
    if (result?.success) {
      // Not sorting client-side since the server performs this to ensure pagination is consistent.
      // ...although we are requesting all data at this stage.
      const paged: PagedExpense = {
        ...result.value,
      };

      return new SuccessResult(paged);
    }

    // type narrowed to FailResult<FailResultBase> since result cannot be undefined at this point
    return result;
  }, [result]);

  // Returns isLoading, isError, error, and data (and anything else from React Query)
  return { ...query, data };
};

const useApiGetExpenseById = (id: string, options?: ByIdOptions) => {
  const query = useGet<Expense>(`/expenses/${id}`, ['expenses', id], {
    staleTime: options?.forceFresh ? 0 : undefined,
    refetchOnMount: options?.forceFresh ? 'always' : undefined,
    usePreviousAsPlaceholder: options?.forceFresh ? false : undefined,
  });

  // Cast to Result<Expense, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
  // This allows TypeScript to infer that when !success, error must exist, and when success, value must exist.
  // Without the cast, TypeScript can't determine the relationship between success and error/value properties.
  return {
    ...query,
    data: query.data as Result<Expense, FailResultBase>,
  };
};

const useApiCreateExpense = () => {
  // Server create response also includes canonical accrualStart field.
  // This mutation remains identity-typed because current callers navigate away and refresh expenses.
  const mutation = usePost<Identity, CreateExpense>('/expenses');

  // Returning the mutation data as Result<Identity, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
};

// Returning the mutation data as Result<Identity, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
const useApiUpdateExpense = () => {
  // Server update response also includes canonical accrualStart field
  // This mutation remains identity-typed because current callers rely on invalidation/refetch.
  const mutation = usePutWithId<Identity, EditExpense>(
    (id: string) => `/expenses/${id}`,
  );
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

const useApiToggleExcludeExpenses = () => {
  const mutation = usePost<void, ToggleExcludeExpenses>(
    '/expenses/toggleExclude',
  );

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
};

const useApiRenewExpenses = () => {
  const mutation = usePost<void, RenewExpenses>('/expenses/renew');

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
  useApiRenewExpenses,
  useApiToggleExcludeExpenses,
  useApiUpdateExpense,
};
