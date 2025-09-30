import { useMemo } from 'react';

import {
  Account,
  compareAccountBsbNumber,
  CreateAccount,
  EditAccount,
  Identity,
} from '@/data';
import { FailResultBase, Result, SuccessResult } from '@/lib';

import { useDelete, useGet, usePost, usePutWithId } from './useApi';

const useApiGetAllAccounts = () => {
  const query = useGet<Account[]>('/accounts', ['accounts']);
  const result = query.data as Result<Account[], FailResultBase>;

  const data = useMemo(() => {
    if (result?.success) {
      // spreading [...result.value] to create a shallow copy of the array since
      // sort() mutates the source array in the react-query cache.
      const sortedResults = [...result.value].sort(compareAccountBsbNumber);
      return new SuccessResult(sortedResults);
    }

    // type narrowed to FailResult<FailResultBase> since result cannot be undefined at this point
    return result;
  }, [result]);

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

const useApiCreateAccount = () => {
  const mutation = usePost<Identity, CreateAccount>('/accounts');

  // Returning the mutation data as Result<Identity, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
};

// Returning the mutation data as Result<Identity, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
const useApiUpdateAccount = () => {
  const mutation = usePutWithId<Identity, EditAccount>(
    (id: string) => `/accounts/${id}`,
  );

  return {
    ...mutation,
    data: mutation.data as Result<Identity, FailResultBase>,
  };
};

// Returning the mutation data as Result<void, FailResultBase> type to enable TypeScript's discriminated union type narrowing.
const useApiDeleteAccount = (id: string) => {
  const mutation = useDelete<void>(`/accounts/${id}`);

  return {
    ...mutation,
    data: mutation.data as Result<void, FailResultBase>,
  };
};

export {
  useApiCreateAccount,
  useApiDeleteAccount,
  useApiGetAccountById,
  useApiGetAllAccounts,
  useApiUpdateAccount,
};
