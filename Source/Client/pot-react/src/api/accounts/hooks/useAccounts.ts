import {
  Account,
  compareAccountBsbNumber,
  CreateAccount,
  EditAccount,
} from '@/data/accounts/account';
import { FailResultBase } from '@/lib/result/failResultBase';
import { Result, SuccessResult } from '@/lib/result/result';

import { useDelete, useGet, usePost, usePut } from '../../hooks/useApi';

const useApiGetAllAccounts = () => {
  const query = useGet<Account[]>('/accounts', ['accounts']);
  const result = query.data as Result<Account[], FailResultBase>;

  let data: Result<Account[], FailResultBase>;

  // useGet() uses React Query which:
  // - returns undefined immediately and while loading
  // - returns the result when the query is successful
  if (result?.success) {
    // spreading [...result.value] to create a shallow copy of the array since sort() mutates the source array
    const sorted = [...result.value].sort(compareAccountBsbNumber);
    data = new SuccessResult(sorted);
  } else {
    // type narrowed to FailResult<FailResultBase> since result cannot be undefined at this point
    data = result;
  }

  // Returns isLoading, isError, error, and data (and anything else from React Query)
  return { ...query, data };
};

const useApiGetAccountById = (id: string) => {
  const query = useGet<Account>(`/accounts/${id}`, ['accounts', id]);

  // Cast to Result<Account, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
  // This allows TypeScript to infer that when !success, error must exist, and when success, value must exist.
  // Without the cast, TypeScript can't determine the relationship between success and error/value properties.
  return {
    ...query,
    data: query.data as Result<Account, FailResultBase>,
  };
};

// Not performing type narrowing as this method currently returns a mutation without Result<T> handling
const useApiCreateAccount = () => {
  return usePost<void, CreateAccount>('/accounts');
};

// Not performing type narrowing as this method currently returns a mutation without Result<T> handling
const useApiUpdateAccount = () => {
  return usePut<void, EditAccount>('/accounts');
};

// Not performing type narrowing as this method currently returns a mutation without Result<T> handling
const useApiDeleteAccount = (id: string) => {
  return useDelete<void>(`/accounts/${id}`);
};

export {
  useApiCreateAccount,
  useApiDeleteAccount,
  useApiGetAccountById,
  useApiGetAllAccounts,
  useApiUpdateAccount,
};
