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
    const sorted = [...result.value].sort(compareAccountBsbNumber);
    data = new SuccessResult(sorted);
  } else {
    data = result;
  }

  // Returns isLoading, isError, error, and data (and anything else from React Query)
  return { ...query, data };
};

// const useApiGetAccountById = (id: string) => {
//   return useGet<Account>(`/accounts/${id}`, ['accounts', id]);
// };

const useApiCreateAccount = () => {
  return usePost<void, CreateAccount>('/accounts');
};

const useApiUpdateAccount = () => {
  return usePut<void, EditAccount>('/accounts');
};

const useApiDeleteAccount = (id: string) => {
  return useDelete<void>(`/accounts/${id}`);
};

export {
  useApiCreateAccount,
  useApiDeleteAccount,
  //useApiGetAccountById,
  useApiGetAllAccounts,
  useApiUpdateAccount,
};
