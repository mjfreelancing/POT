import {
  Account,
  compareAccountBsbNumber,
  CreateAccount,
  EditAccount,
} from '@/data/accounts/account';

import { useDelete, useGet, usePost, usePut } from '../../hooks/useApi';

const useApiGetAllAccounts = () => {
  const { data, ...rest } = useGet<Account[]>('/accounts', ['accounts']);
  const sorted = data?.sort(compareAccountBsbNumber);
  return { data: sorted, ...rest };
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
