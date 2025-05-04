import {
  Account,
  compareAccountBsbNumber,
  CreateAccount,
  EditAccount,
} from '@/data/accounts/account';
import { FailResultBase } from '@/lib/result/failResultBase';
import { Result } from '@/lib/result/result';

import { useDelete, useGet, usePost, usePut } from '../../hooks/useApi';

const useApiGetAllAccounts = () => {
  const query = useGet<Account[]>('/accounts', ['accounts']);
  const result = query.data as Result<Account[], FailResultBase>;

  // In React Query, when data is loading or there's an error, `query.data` will be `undefined`
  const accounts = result?.success ? result.value : undefined;
  const sorted = accounts?.sort(compareAccountBsbNumber);

  return { ...query, data: sorted };
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
